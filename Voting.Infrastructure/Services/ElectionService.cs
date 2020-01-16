using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Nethereum.Util;
using Votin.Model;
using Voting.Model.Context;
using Voting.Model.Entities;
using Votin.Model.Exceptions;
using Voting.Infrastructure.API.Election;
using Voting.Infrastructure.DTO.Election;
using Voting.Infrastructure.Model.Common;
using Voting.Infrastructure.Model.Election;

namespace Voting.Infrastructure.Services
{
    public class ElectionService
    {
        private readonly BlockchainContext _dbContext;
        private readonly IMapper _mapper;

        public ElectionService(BlockchainContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Election> CreateElectionAsync(CreateElection model)
        {
            Election election = _mapper.Map<Election>(model);

            ValidateElectionCandidate(election.Candidates);

            election.Address = EthECKey.GenerateKey().GetPublicAddress();

            _dbContext.Elections.Add(election);

            await _dbContext.SaveChangesAsync();

            return election;
        }

        private void ValidateElectionCandidate(List<ElectionCandidate> electionCandidates)
        {
            List<string> invalidCandidates = new List<string>();

            electionCandidates.ForEach(c =>
            {
                if (!AddressExtensions.IsValidEthereumAddressHexFormat(c.Candidate))
                    invalidCandidates.Add(c.Candidate);
            });

            if (invalidCandidates.Any())
                throw new Voting.Model.Exceptions.ValidationException(
                    "آدرس وارد شده برای کاندیدا ها اشتباه است" + Environment.NewLine +
                    string.Join(" , ", invalidCandidates));
        }

        public async Task<Election> UpdateElectionAsync(UpdateElection election)
        {
            Election old = _dbContext.Elections
                .Include(e => e.Candidates)
                .SingleOrDefault(e => e.Id == election.Id);

            if (old == null)
                throw new NotFoundException("انتخابات");

            List<ElectionCandidate> candidates = _mapper.Map<List<ElectionCandidate>>(election.Candidates);

            ValidateElectionCandidate(candidates);

            _dbContext.RemoveRange(old.Candidates);
            await _dbContext.ElectionCandidates.AddRangeAsync(candidates);

            old.Name = election.Name;

            _dbContext.Elections.Update(old);

            await _dbContext.SaveChangesAsync();

            return old;
        }

        public async Task RemoveElectionAsync(int electionId)
        {
            Election election = await _dbContext.Elections.SingleOrDefaultAsync(e => e.Id == electionId);

            if (election == null)
                throw new NotFoundException("انتخابات");

            _dbContext.Elections.Remove(election);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<PagedResult<ElectionDTO>> GetElectionsAsync(ElectionSearch model)
        {
            PagedResult<ElectionDTO> result = new PagedResult<ElectionDTO>();

            model.Name = model.Name ?? "";
            model.Address = model.Address ?? "";

            var elections = _dbContext.Elections
                .Where(e => e.Name.Contains(model.Name) &&
                            e.Address.Contains(model.Address))
                .OrderByDescending(e => e.InsertDate)
                .ProjectTo<ElectionDTO>(_mapper.ConfigurationProvider);

            result.TotalCount = elections.Count();

            result.Items = await elections.ToListAsync();

            return result;
        }

        public async Task<Election> AddCandidateToElectionAsync(int electionId, string candidateAddress)
        {
            Election election = await _dbContext.Elections
                .Include(e => e.Candidates)
                .SingleOrDefaultAsync(e => e.Id == electionId);

            if (election == null)
                throw new NotFoundException("انتخابات");

            if (!election.Candidates.Any(c => c.Candidate == candidateAddress))
            {
                ElectionCandidate candidate = new ElectionCandidate
                {
                    ElectionId = electionId,
                    Candidate = candidateAddress
                };

                _dbContext.ElectionCandidates.Add(candidate);

                await _dbContext.SaveChangesAsync();

                election.Candidates.Add(candidate);
            }

            return election;
        }

        public async Task<ElectionDTO> GetElectionAsync(int electionId)
        {
            ElectionDTO election = await _dbContext.Elections
                .Include(e => e.Candidates)
                .ProjectTo<ElectionDTO>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(e => e.Id == electionId);

            if (election == null)
                throw new NotFoundException("انتخابات");

            return election;
        }

        public async Task<List<ElectionDTO>> GetUnvotedElections(string voterPublicKey)
        {
            List<Election> allElections = await _dbContext.Elections
                .Include(e => e.Candidates)
                .ToListAsync();

            List<string> votedElectionAddresses = BlockChain.Chain
                .Where(b => b.Data != null)
                .SelectMany(b => b.Data.Select(t => t.Input))
                .Where(t => t.Address == voterPublicKey)
                .Select(t => t.Address).ToList();

            allElections.RemoveAll(e => votedElectionAddresses.Contains(e.Address));

            return _mapper.Map<List<ElectionDTO>>(allElections);
        }
    }
}
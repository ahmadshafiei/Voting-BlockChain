using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Votin.Model;
using Voting.Model.Context;
using Voting.Model.Entities;
using Votin.Model.Exceptions;
using Voting.Infrastructure.DTO.Election;
using Voting.Infrastructure.Model.Common;
using Voting.Infrastructure.Model.Election;

namespace Voting.Infrastructure.Services
{
    public class ElectionService
    {
        private readonly BlockchainContext _dbContext;

        public ElectionService(BlockchainContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Election> CreateElectionAsync(Election election)
        {
            election.Address = EthECKey.GenerateKey().GetPublicAddress();

            _dbContext.Elections.Add(election);

            await _dbContext.SaveChangesAsync();

            return election;
        }

        public async Task<Election> UpdateElectionAsync(Election election)
        {
            Election old = _dbContext.Elections.SingleOrDefault(e => e.Id == election.Id);

            if (old == null)
                throw new NotFoundException("انتخابات");

            old.Name = election.Name;
            old.Candidates = election.Candidates;

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
                .Select(e => new ElectionDTO
                {
                    Id =  e.Id,
                    Name = e.Name,
                    Address = e.Address,
                    Candidates = string.Join(",", e.Candidates.Select(c => c.Candidate))
                });

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

        public async Task<Election> GetElectionAsync(int electionId)
        {
            Election election = await _dbContext.Elections.SingleOrDefaultAsync(e => e.Id == electionId);

            if (election == null)
                throw new NotFoundException("انتخابات");

            return election;
        }
    }
}
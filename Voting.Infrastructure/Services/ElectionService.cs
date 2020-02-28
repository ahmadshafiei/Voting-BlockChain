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
using Microsoft.EntityFrameworkCore.Internal;
using Nethereum.Util;
using Newtonsoft.Json;
using Votin.Model;
using Voting.Model.Context;
using Voting.Model.Entities;
using Votin.Model.Exceptions;
using Voting.Infrastructure.API.Election;
using Voting.Infrastructure.DTO.Election;
using Voting.Infrastructure.Model.Common;
using Voting.Infrastructure.Model.Election;
using Transaction = Voting.Model.Entities.Transaction;
using ValidationException = Voting.Model.Exceptions.ValidationException;

namespace Voting.Infrastructure.Services
{
    public class ElectionService
    {
        private readonly BlockchainCommonContext _commonDbContext;
        private readonly BlockchainContext _dbContext;
        private readonly IMapper _mapper;

        public ElectionService(BlockchainCommonContext commonDbContext, BlockchainContext dbContext, IMapper mapper)
        {
            _commonDbContext = commonDbContext;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Election> CreateElectionAsync(CreateElection model)
        {
            Election election = _mapper.Map<Election>(model);

            ValidateElectionCandidate(election.Candidates);

            election.Address = EthECKey.GenerateKey().GetPublicAddress();
            election.Status = ElectionStatus.Pending;

            _commonDbContext.Elections.Add(election);

            await _commonDbContext.SaveChangesAsync();

            return election;
        }

        private void ValidateElectionCandidate(List<ElectionCandidate> electionCandidates)
        {
            List<string> invalidCandidates = new List<string>();

            electionCandidates.ForEach(c =>
            {
                if (c.Candidate.Length != 130 || c.Candidate.Any(ch =>
                    !((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z'))))
                    invalidCandidates.Add(c.Candidate);
            });

            if (invalidCandidates.Any())
                throw new Voting.Model.Exceptions.ValidationException(
                    "آدرس وارد شده برای کاندیدا ها اشتباه است" + Environment.NewLine +
                    string.Join(" , ", invalidCandidates));
        }

        public async Task<Election> UpdateElectionAsync(UpdateElection election)
        {
            Election old = _commonDbContext.Elections
                .Include(e => e.Candidates)
                .SingleOrDefault(e => e.Id == election.Id);

            if (old == null)
                throw new NotFoundException("انتخابات");

            bool usedInBlockchain = _dbContext.Blocks
                .ToList()
                .SelectMany(b => JsonConvert.DeserializeObject<List<Transaction>>(b.Data))
                .Any(t => t.Outputs.Any(o => o.ElectionAddress == old.Address));

            bool usedInTransactions =
                await _dbContext.Transactions.AnyAsync(t => t.Outputs.Any(o => o.ElectionAddress == old.Address));

            if (usedInBlockchain || usedInTransactions)
                throw new ValidationException("در این انتخابات رای گیری انجام شده و امکان ویرایش آن نیست");


            List<ElectionCandidate> candidates = _mapper.Map<List<ElectionCandidate>>(election.Candidates);

            ValidateElectionCandidate(candidates);

            _commonDbContext.RemoveRange(old.Candidates);
            await _commonDbContext.ElectionCandidates.AddRangeAsync(candidates);

            old.Name = election.Name;

            _commonDbContext.Elections.Update(old);

            await _commonDbContext.SaveChangesAsync();

            return old;
        }

        public async Task RemoveElectionAsync(int electionId)
        {
            Election election = await _commonDbContext.Elections.SingleOrDefaultAsync(e => e.Id == electionId);

            if (election == null)
                throw new NotFoundException("انتخابات");

            bool usedInBlockchain = _dbContext.Blocks
                .ToList()
                .SelectMany(b => JsonConvert.DeserializeObject<List<Transaction>>(b.Data))
                .Any(t => t.Outputs.Any(o => o.ElectionAddress == election.Address));

            bool usedInTransactions =
                await _dbContext.Transactions.AnyAsync(t => t.Outputs.Any(o => o.ElectionAddress == election.Address));

            if (usedInBlockchain || usedInTransactions)
                throw new ValidationException("در این انتخابات رای گیری انجام شده و امکان حذف آن نیست");

            _commonDbContext.Elections.Remove(election);

            await _commonDbContext.SaveChangesAsync();
        }

        public async Task<PagedResult<ElectionDTO>> GetElectionsAsync(ElectionSearch model)
        {
            PagedResult<ElectionDTO> result = new PagedResult<ElectionDTO>();

            model.Name = model.Name ?? "";
            model.Address = model.Address ?? "";

            var elections = await _commonDbContext.Elections
                .Where(e => e.Name.Contains(model.Name) &&
                            e.Address.Contains(model.Address))
                .OrderByDescending(e => e.InsertDate)
                .ProjectTo<ElectionDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var candidates = await _commonDbContext.Users
                .Where(u =>
                    elections.SelectMany(e => e.Candidates).Select(c => c.CandidateAddress).Contains(u.PublicKey))
                .ToListAsync();

            result.TotalCount = elections.Count();

            result.Items = elections;

            foreach (var candidate in result.Items.SelectMany(i => i.Candidates))
            {
                candidate.CandidateName =
                    candidates.Any(c => c.PublicKey == candidate.CandidateAddress)
                        ? candidates.Single(c => c.PublicKey == candidate.CandidateAddress).Name
                        : "";
            }

            return result;
        }

        public async Task<Election> AddCandidateToElectionAsync(int electionId, string candidateAddress)
        {
            Election election = await _commonDbContext.Elections
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

                _commonDbContext.ElectionCandidates.Add(candidate);

                await _commonDbContext.SaveChangesAsync();

                election.Candidates.Add(candidate);
            }

            return election;
        }

        public async Task<ElectionDTO> GetElectionAsync(int electionId)
        {
            ElectionDTO election = await _commonDbContext.Elections
                .Include(e => e.Candidates)
                .ProjectTo<ElectionDTO>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(e => e.Id == electionId);

            if (election == null)
                throw new NotFoundException("انتخابات");

            return election;
        }

        public async Task<List<ElectionDTO>> GetUnvotedElections(string voterPublicKey)
        {
            List<Election> allElections = await _commonDbContext.Elections
                .Where(e => e.Status == ElectionStatus.Pending)
                .Include(e => e.Candidates)
                .ToListAsync();

            List<string> votedElectionAddresses = _dbContext.Blocks
                .ToList()
                .SelectMany(b => JsonConvert.DeserializeObject<List<Transaction>>(b.Data))
                .Where(t => t.Input.Address == voterPublicKey)
                .SelectMany(t => t.Outputs.Select(o => o.ElectionAddress)).ToList();

            List<string> transactionVotes = await _dbContext.Transactions
                .Where(t => t.Input.Address == voterPublicKey)
                .SelectMany(t => t.Outputs.Select(o => o.ElectionAddress))
                .ToListAsync();

            votedElectionAddresses.AddRange(transactionVotes);

            allElections.RemoveAll(e => votedElectionAddresses.Contains(e.Address));

            allElections.ForEach(e => e.Candidates.RemoveAll(c => c.Candidate == voterPublicKey));

            allElections.RemoveAll(e => !e.Candidates.Any());

            var result = _mapper.Map<List<ElectionDTO>>(allElections);


            foreach (var c in result.SelectMany(e => e.Candidates))
            {
                var user = await _commonDbContext.Users.SingleOrDefaultAsync(u => u.PublicKey == c.CandidateAddress);
                c.CandidateName = user?.Name;
            }

            return result;
        }

        public async Task<List<ParticipatedElection>> GetParticipatedElectionsAsync(string voterAddress)
        {
            List<ParticipatedElection> participatedElections = _dbContext.Blocks
                .ToList()
                .SelectMany(b => JsonConvert.DeserializeObject<List<Transaction>>(b.Data))
                .Where(i => i.Input.Address == voterAddress)
                .SelectMany(t => t.Outputs)
                .Select(o => new ParticipatedElection
                {
                    ElectionAddress = o.ElectionAddress,
                    CandidateAddress = o.CandidateAddress
                }).ToList();

            foreach (var participatedElection in participatedElections)
            {
                Election election = await _commonDbContext.Elections.SingleOrDefaultAsync(e =>
                    e.Address == participatedElection.ElectionAddress);

                if (election != null)
                    participatedElection.ElectionName = election.Name;

                var candidate =
                    await _commonDbContext.Users.SingleOrDefaultAsync(u =>
                        u.PublicKey == participatedElection.CandidateAddress);

                if (candidate != null)
                    participatedElection.CandidateName = candidate.Name;
            }

            return participatedElections;
        }

        public async Task<List<CandidatedElection>> CandidatedElectionAsync(string publicKey)
        {
            List<Transaction> transactions = _dbContext.Blocks
                .ToList()
                .SelectMany(b => JsonConvert.DeserializeObject<List<Transaction>>(b.Data))
                .ToList();

            transactions.ForEach(t => t.Outputs.ForEach(o => o.Transaction = t));

            List<CandidatedElection> candidatedElections =
                transactions
                    .SelectMany(t => t.Outputs)
                    .Where(o => o.CandidateAddress == publicKey)
                    .GroupBy(o => o.ElectionAddress)
                    .Select(e => new CandidatedElection
                    {
                        ElectionAddress = e.Key,
                        Voters = e.Select(o => o.Transaction.Input.Address).ToList()
                    })
                    .ToList();

            foreach (var candidatedElection in candidatedElections)
            {
                Election election = await _commonDbContext.Elections.SingleOrDefaultAsync(e =>
                    e.Address == candidatedElection.ElectionAddress);

                if (election != null)
                    candidatedElection.ElectionName = election.Name;
            }

            return candidatedElections;
        }

        public async Task<List<ElectionVotes>> GetELectionVotesAsync()
        {
            List<ElectionVotes> result = new List<ElectionVotes>();

            var groupedElections = _dbContext.Blocks
                .ToList()
                .SelectMany(b => JsonConvert.DeserializeObject<List<Transaction>>(b.Data))
                .SelectMany(t => t.Outputs)
                .GroupBy(o => o.ElectionAddress)
                .ToList();

            foreach (var electionGroup in groupedElections)
            {
                var election =
                    await _commonDbContext.Elections.SingleOrDefaultAsync(e => e.Address == electionGroup.Key);

                var candidates = await _commonDbContext.Users
                    .Where(u => electionGroup.Select(g => g.CandidateAddress).Contains(u.PublicKey)).ToListAsync();

                result.Add(new ElectionVotes
                {
                    Election = election.Name,
                    ElectionStatus = election.Status,
                    Candidates = electionGroup.GroupBy(e => e.CandidateAddress).Select(c => new CandidateVote
                    {
                        Candidate = candidates.Any(cc => cc.PublicKey == c.Key && !string.IsNullOrEmpty(cc.Name))
                            ? candidates.Single(cc => cc.PublicKey == c.Key).Name
                            : c.Key,
                        TotalVotes = c.Count()
                    }).ToList()
                });
            }

            return result;
        }

        public async Task CloseElectionAsync(int electionId)
        {
            Election election = await _commonDbContext.Elections.SingleOrDefaultAsync(e => e.Id == electionId);

            if (election == null)
                throw new NotFoundException("انتخابات");

            election.Status = ElectionStatus.Closed;

            _commonDbContext.Update(election);

            await _commonDbContext.SaveChangesAsync();
        }

        public async Task OpenElectionAsync(int electionId)
        {
            Election election = await _commonDbContext.Elections.SingleOrDefaultAsync(e => e.Id == electionId);

            if (election == null)
                throw new NotFoundException("انتخابات");

            election.Status = ElectionStatus.Pending;

            _commonDbContext.Update(election);

            await _commonDbContext.SaveChangesAsync();
        }
    }
}
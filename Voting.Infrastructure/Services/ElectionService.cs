using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Votin.Model;
using Votin.Model.Entities;
using Votin.Model.Exceptions;
using Voting.Infrastructure.DTO.Election;
using Voting.Infrastructure.Model.Common;
using Voting.Infrastructure.Model.Election;

namespace Voting.Infrastructure.Services
{
    public class ElectionService
    {
        public Election CreateElection(Election election)
        {
            election.Address = EthECKey.GenerateKey().GetPublicAddress();

            return election;
        }

        public Election UpdateElection(Election election)
        {
            Election old = Context.Elections.SingleOrDefault(e => e.Address == election.Address);

            if (election == null)
                throw new NotFoundException("انتخابات");

            old.Name = election.Name;
            old.Candidates = election.Candidates;

            return old;
        }

        public void RemoveElection(string address)
        {
            Election election = Context.Elections.SingleOrDefault(e => e.Address == address);

            if (election == null)
                throw new NotFoundException("انتخابات");

            Context.Elections.Remove(election);
        }

        public PagedResult<ElectionDTO> GetElections(ElectionSearch model)
        {
            PagedResult<ElectionDTO> result = new PagedResult<ElectionDTO>();

            var elections = Context.Elections
                .Where(e => e.Name.Contains(model.Name) &&
                            e.Address.Contains(model.Address))
                .OrderByDescending(e => e.InsertDate)
                .Select(e => new ElectionDTO
                {
                    Name = e.Name,
                    Address = e.Address,
                    Candidates = string.Join(",", e.Candidates)
                });

            result.TotalCount = elections.Count();

            result.Items = elections.ToList();

            return result;
        }

        public Election AddCandidateToElection(string electionAddress, string candidateAddress)
        {
            Election election = Context.Elections.SingleOrDefault(e => e.Address == electionAddress);

            if (election == null)
                throw new NotFoundException("انتخابات");

            if (!election.Candidates.Any(c => c == candidateAddress))
                election.Candidates.Add(candidateAddress);

            return election;
        }

        public Election GetElection(string address)
        {
            Election election = Context.Elections.SingleOrDefault(e => e.Address == address);

            if (election == null)
                throw new NotFoundException("انتخابات");

            return election;
        }
    }
}
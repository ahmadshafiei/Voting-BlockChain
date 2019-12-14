using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Votin.Model;
using Votin.Model.Entities;
using Votin.Model.Exceptions;

namespace Voting.Infrastructure.Services
{
    public class ElectionService
    {
        public Election CreateElection(string name)
        {
            Election election = new Election
            {
                Name = name,
                Address = EthECKey.GenerateKey().GetPublicAddress(),
            };

            return election;
        }

        public List<Election> GetElections()
        {
            return Context.Elections;
        }

        public Election AddCandidateToElection(string electionAddress, string candidateAddress)
        {
            Election election = Context.Elections.Where(e => e.Address == electionAddress).SingleOrDefault();

            if (election == null)
                throw new NotFoundException("انتخابات");

            if (!election.Candidates.Any(c => c == candidateAddress))
                election.Candidates.Add(candidateAddress);

            return election;
        }


    }
}

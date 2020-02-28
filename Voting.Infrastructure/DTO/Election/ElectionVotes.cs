using System.Collections.Generic;
using System.Globalization;
using Voting.Model.Entities;

namespace Voting.Infrastructure.DTO.Election
{
    public class ElectionVotes
    {
        public string Election { get; set; }
        public ElectionStatus ElectionStatus { get; set; }
        public List<CandidateVote> Candidates { get; set; }
    }

    public class CandidateVote
    {
        public string Candidate { get; set; }
        public int TotalVotes { get; set; }
    }
}
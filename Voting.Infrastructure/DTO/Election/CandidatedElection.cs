using System.Collections.Generic;

namespace Voting.Infrastructure.DTO.Election
{
    public class CandidatedElection
    {
        public string ElectionAddress { get; set; }
        public string ElectionName { get; set; }
        public List<string> Vouters { get; set; }
    }
}
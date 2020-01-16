using System.Collections.Generic;

namespace Voting.Infrastructure.API.Election
{
    public class UpdateElection
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<UpdateElectionCandidate> Candidates { get; set; }
    }

    public class UpdateElectionCandidate
    {
        public int ElectionId { get; set; }
        public string Candidate { get; set; }
    }
}
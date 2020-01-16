using System.Collections.Generic;
using System.Security.Principal;

namespace Voting.Infrastructure.API.Election
{
    public class CreateElection
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public List<CreateElectionCandidate> Candidates { get; set; }
    }

    public class CreateElectionCandidate
    {
        public string Candidate { get; set; }
    }
}
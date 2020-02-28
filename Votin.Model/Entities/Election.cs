using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace Voting.Model.Entities
{
    public class Election
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public ElectionStatus Status { get; set; }
        public DateTime InsertDate { get; set; } = DateTime.Now;
        public List<ElectionCandidate> Candidates { get; set; } = new List<ElectionCandidate>();
    }

    public enum ElectionStatus
    {
        Pending = 1,
        Closed = 2
    }
}
using System.Collections.Generic;
using Voting.Model.Entities;

namespace Voting.Infrastructure.DTO.Election
{
    public class ElectionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public List<ElectionCandidateDTO> Candidates { get; set; }
    }

    public class ElectionCandidateDTO
    {
        public int Id { get; set; }
        public int ElectionId { get; set; }
        public string Candidate { get; set; }
    }
}
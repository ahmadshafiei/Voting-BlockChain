using System.ComponentModel.DataAnnotations.Schema;

namespace Voting.Model.Entities
{
    public class ElectionCandidate
    {
        public int Id { get; set; }
        
        public int ElectionId { get; set; }

        [ForeignKey(nameof(ElectionId))] 
        public Election Election { get; set; }

        public string Candidate { get; set; }
    }
}
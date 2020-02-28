namespace Voting.Infrastructure.DTO.Election

{
    public class ParticipatedElection
    {
        public string ElectionAddress { get; set; }
        public string ElectionName { get; set; }
        public string CandidateAddress{ get; set; }
        public string CandidateName{ get; set; }
    }
}
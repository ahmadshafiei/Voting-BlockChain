using System;
using System.Security.Cryptography;


namespace Voting.Model.Entities
{
    public class TransactionOutput
    {
        public DateTime Timestamp { get; set; }
        public string ElectionAddress { get; set; }
        public string CandidateAddress { get; set; }

        public TransactionOutput(string electionAddress, string candidateAddress)
        {
            Timestamp = DateTime.Now;
            ElectionAddress = electionAddress;
            CandidateAddress = candidateAddress;
        }
    }
}

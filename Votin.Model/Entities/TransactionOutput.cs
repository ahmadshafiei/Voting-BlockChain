using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;


namespace Voting.Model.Entities
{
    public class TransactionOutput
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string ElectionAddress { get; set; }
        public string CandidateAddress { get; set; }

        public int TransactionId { get; set; }
        
        [ForeignKey(nameof(TransactionId))] 
        public Transaction Transaction { get; set; }

        public TransactionOutput(string electionAddress, string candidateAddress)
        {
            Timestamp = DateTime.Now;
            ElectionAddress = electionAddress;
            CandidateAddress = candidateAddress;
        }
    }
}
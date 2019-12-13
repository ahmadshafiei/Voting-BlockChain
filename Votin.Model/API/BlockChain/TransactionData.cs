using System.Security.Cryptography;

namespace Voting.Model.API.BlockChain
{
    public class TransactionData
    {
        public string ElectionAddress { get; set; }
        public string CandidateAddress { get; set; }
    }
}

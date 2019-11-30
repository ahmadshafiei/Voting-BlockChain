using System.Security.Cryptography;

namespace Voting.Model.API.BlockChain
{
    public class TransactionData
    {
        public int Amount { get; set; }
        public string Recipient { get; set; }
    }
}

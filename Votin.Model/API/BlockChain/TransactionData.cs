using System.Security.Cryptography;

namespace Votin.Model.API.BlockChain
{
    public class TransactionData
    {
        public int Amount { get; set; }
        public string Recipient { get; set; }
    }
}

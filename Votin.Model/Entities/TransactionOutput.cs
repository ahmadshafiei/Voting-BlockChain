using System.Security.Cryptography;


namespace Voting.Model.Entities
{
    public class TransactionOutput
    {
        public int Amount { get; set; }
        public string Address { get; set; }

        public TransactionOutput(int amount, string address)
        {
            Amount = amount;
            Address = address;
        }
    }
}

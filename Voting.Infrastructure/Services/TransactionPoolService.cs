using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Votin.Model.Entities;

namespace Voting.Infrastructure.Services
{
    public class TransactionPoolService
    {
        public List<Transaction> Transactions = new List<Transaction>();

        public void UpdateOrAddTransaction(Transaction transaction)
        {
            bool editMode = Transactions.Any(t => t.Id == transaction.Id);

            if (editMode)
                Transactions[Transactions.FindIndex(t => t.Id == transaction.Id)] = transaction;
            else
                Transactions.Add(transaction);
        }

        public Transaction ExistingTransaction(string publicKey)
        {
            return Transactions.SingleOrDefault(t => t.Input.Address == publicKey);
        }
    }
}

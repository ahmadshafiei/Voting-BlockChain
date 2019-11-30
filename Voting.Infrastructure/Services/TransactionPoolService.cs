using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Voting.Model.Entities;

namespace Voting.Infrastructure.Services
{
    public class TransactionPoolService
    {
        private readonly TransactionService _transactionService;

        public List<Transaction> Transactions = new List<Transaction>();

        public TransactionPoolService(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

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

        public List<Transaction> GetValidTransactions()
        {
            return Transactions.Where(t =>
            {
                int outputTotal = t.Outputs.Select(o => o.Amount).Aggregate(0, (x, y) => x + y);

                if(t.Input.Amount != outputTotal)
                {
                    Console.WriteLine($"Invalid transaction from {t.Input.Address}");
                    return false;
                }

                if(!_transactionService.VerifyTransaction(t))
                {
                    Console.WriteLine($"Invalid signature from {t.Input.Address}");
                    return false;
                }

                return true;
            }).ToList();
        }

        public void ClearPool()
        {
            Transactions = new List<Transaction>();
        }
    }
}

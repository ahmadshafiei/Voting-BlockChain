using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Model.Context;
using Voting.Model.Entities;

namespace Voting.Infrastructure.Services
{
    public class TransactionPoolService
    {
        private readonly TransactionService _transactionService;
        private readonly BlockchainContext _dbContext;

        public TransactionPoolService(TransactionService transactionService, BlockchainContext dbContext)
        {
            _transactionService = transactionService;
            _dbContext = dbContext;
        }

        public void UpdateOrAddTransaction(Transaction transaction)
        {
            bool editMode = _dbContext.Transactions.Any(t => t.Id == transaction.Id);

            if (editMode)
                _dbContext.Update(transaction);
            else
                _dbContext.Add(transaction);

            _dbContext.SaveChanges();
        }

        public async Task UpdateOrAddTransactionAsync(Transaction transaction)
        {
            bool editMode = await _dbContext.Transactions.AnyAsync(t => t.Id == transaction.Id);

            if (editMode)
                _dbContext.Update(transaction);
            else
                _dbContext.Add(transaction);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<Transaction> ExistingTransaction(string publicKey)
        {
            Transaction transaction = await _dbContext.Transactions
                .Include(t => t.Input)
                .Include(t => t.Outputs)
                .SingleOrDefaultAsync(t => t.Input.Address == publicKey);

            return transaction;
        }

        public async Task<List<Transaction>> GetValidTransactions()
        {
            List<Transaction> transactions = await _dbContext.Transactions.ToListAsync();

            return transactions.Where(t =>
            {
                if (!_transactionService.VerifyTransaction(t))
                {
                    Console.WriteLine($"Invalid signature from {t.Input.Address}");
                    return false;
                }

                return true;
            }).ToList();
        }

        public async Task ClearPool()
        {
            _dbContext.Transactions.RemoveRange();
            await _dbContext.SaveChangesAsync();
        }
    }
}
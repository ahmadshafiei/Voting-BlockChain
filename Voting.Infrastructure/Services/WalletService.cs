using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Voting.Model.Context;
using Voting.Model.Entities;

namespace Voting.Infrastructure.Services
{
    public class WalletService
    {
        private readonly TransactionService _transactionService;
        private readonly BlockchainContext _dbContext;

        public WalletService(TransactionService transactionService,BlockchainContext dbContext)
        {
            _transactionService = transactionService;
            _dbContext = dbContext;
        }

        public async Task<Transaction> CreateTransaction(Wallet wallet, string electionAddress, string candidateAddress,
            TransactionPoolService transactionPool)
        {
            Transaction transaction = await transactionPool.ExistingTransaction(wallet.PublicKey);

            if (transaction == null)
            {
                transaction = _transactionService.NewTransaction(wallet, electionAddress, candidateAddress);
            }
            else
                _transactionService.UpdateTransaction(transaction, wallet, electionAddress, candidateAddress);

            await transactionPool.UpdateOrAddTransaction(transaction);

            return transaction;
        }

        /// <summary>
        /// Calculate total votes for a candidate(or anyone actually) in the particular election
        /// </summary>
        public int CalculateBalance(Wallet wallet, string electionAddress)
        {
            int balance = wallet.Balance;

            List<Transaction> votes = BlockChain.Chain.SelectMany(c => c.Data).ToList();

            balance += votes.SelectMany(v => v.Outputs)
                .Where(o => o.CandidateAddress == wallet.PublicKey && o.ElectionAddress == electionAddress).Count();

            return balance;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Voting.Model.Entities;

namespace Voting.Infrastructure.Services
{
    public class WalletService
    {
        private readonly TransactionService _transactionService;

        public WalletService(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        public Transaction CreateTransaction(Wallet wallet, string electionAddress, string candidateAddress, TransactionPoolService transactionPool)
        {
            Transaction transaction = transactionPool.ExistingTransaction(wallet.PublicKey);

            if (transaction == null)
                transaction = _transactionService.NewTransaction(wallet, electionAddress, candidateAddress);
            else
                _transactionService.UpdateTransaction(transaction, wallet, electionAddress, candidateAddress);

            transactionPool.UpdateOrAddTransaction(transaction);

            return transaction;
        }

        /// <summary>
        /// Calculate total votes for a candidate(or anyone actually) in the particular election
        /// </summary>
        public int CalculateBalance(Wallet wallet, string electionAddress)
        {
            int balance = wallet.Balance;

            List<Transaction> votes = BlockChain.Chain.SelectMany(c => c.Data).ToList();

            balance += votes.SelectMany(v => v.Outputs).Where(o => o.CandidateAddress == wallet.PublicKey && o.ElectionAddress == electionAddress).Count();

            return balance;

        }
    }
}

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

        public Transaction CreateTransaction(Wallet wallet, string recipient, int amount,TransactionPoolService transactionPool)
        {
            wallet.Balance = CalculateBalance(wallet);
            Transaction transaction = transactionPool.ExistingTransaction(wallet.PublicKey);

            if (transaction == null)
                transaction = _transactionService.NewTransaction(wallet, recipient, amount);
            else
                _transactionService.UpdateTransaction(transaction, wallet, recipient, amount);

            transactionPool.UpdateOrAddTransaction(transaction);

            return transaction;
        }

        /// <summary>
        /// Which gives the total votes of a person
        /// </summary>
        public int CalculateBalance(Wallet wallet)
        {
            int balance = wallet.Balance;

            List<Transaction> transactions = BlockChain.Chain
                .SelectMany(b => b.Data)
                .ToList();

            List<Transaction> walletTransactions = transactions
                .Where(t => t.Input.Address == wallet.PublicKey)
                .ToList();

            DateTime startTime = DateTime.MinValue;

            Transaction mostRecentTransaction = walletTransactions
                .Aggregate((prev, curr) => prev.Input.TimeStamp > curr.Input.TimeStamp ? prev : curr);

            if (mostRecentTransaction != null)
            {
                balance = mostRecentTransaction.Outputs.Single(o => o.Address == wallet.PublicKey).Amount;
                startTime = mostRecentTransaction.Input.TimeStamp;
            }


            transactions.ForEach(t =>
            {
                if (t.Input.TimeStamp > startTime)
                    t.Outputs.ForEach(o =>
                    {
                        if (o.Address == wallet.PublicKey)
                            balance += o.Amount;
                    });
            });

            return balance;

        }

        public static Wallet BlockchainWallet()
        {
            return new Wallet
            {
                PublicKey = "blockchain-wallet"
            };
        }
    }
}

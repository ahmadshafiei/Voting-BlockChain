using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Votin.Model.Entities;

namespace Voting.Infrastructure.Services
{
    public class WalletService
    {
        private readonly TransactionService _transactionService;

        public WalletService(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        public Transaction CreateTransaction(Wallet wallet, string recipient, int amount, TransactionPoolService transactionPool)
        {
            Transaction transaction = transactionPool.ExistingTransaction(wallet.PublicKey);

            if (transaction == null)
                transaction = _transactionService.NewTransaction(wallet, recipient, amount);
            else
                _transactionService.UpdateTransaction(transaction, wallet, recipient, amount);

            transactionPool.UpdateOrAddTransaction(transaction);

            return transaction;
        }
    }
}

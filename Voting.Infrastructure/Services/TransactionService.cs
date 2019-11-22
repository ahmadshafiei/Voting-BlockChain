using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Votin.Model.Entities;
using Votin.Model.Exceptions;
using Voting.Infrastructure.Utility;

namespace Voting.Infrastructure.Services
{
    public class TransactionService
    {
        /// <summary>
        /// TODO: amount should change to vote (actually there should be no amount , vote is defaults to 1)
        /// Implementation change so senders balance don't decrease on sending 
        /// </summary>
        /// <param name="sender">Voter</param>
        /// <param name="recipient">Elected person</param>
        /// <param name="amount">Vote count (actually it should be 1)</param>
        public Transaction NewTransaction(Wallet sender, string recipient, int amount)
        {
            if (sender.Balance < amount)
                throw new InvalidTransactionException("مقدار تراکنش بیشتر از موجودی والت می باشد");

            var transaction = new Transaction
            {
                Outputs = new List<TransactionOutput>
                {
                    new TransactionOutput(sender.Balance - amount , sender.PublicKey),
                    new TransactionOutput(amount ,  recipient)
                }
            };

            SignTransaction(transaction, sender);

            return transaction;
        }

        /// <summary>
        /// See <see cref="NewTransaction"/> Documentation
        /// </summary>
        public Transaction UpdateTransaction(Transaction transaction, Wallet sender, string recipient, int amount)
        {
            TransactionOutput output = transaction.Outputs.Single(o => o.Address == sender.PublicKey);

            if (amount > output.Amount)
                throw new InvalidTransactionException("مقدار تراکنش بیشتر از موجودی والت می باشد");

            output.Amount -= amount;

            transaction.Outputs.Add(new TransactionOutput(amount, recipient));

            SignTransaction(transaction, sender);

            return transaction;
        }

        public void SignTransaction(Transaction transaction, Wallet sender)
        {
            transaction.Input = new TransactionInput
            {
                TimeStamp = DateTime.Now,
                Amount = sender.Balance,
                Address = sender.PublicKey,
                Signature = sender.Sign(Hash.HashTransactionOutput(transaction.Outputs.ToArray()))
            };
        }

        public bool VerifyTransaction(Transaction transaction)
        {
            return ECCUtility.VerifySignature(
                transaction.Input.Address,
                transaction.Input.Signature,
                Hash.HashTransactionOutput(transaction.Outputs.ToArray())
            );
        }
    }
}

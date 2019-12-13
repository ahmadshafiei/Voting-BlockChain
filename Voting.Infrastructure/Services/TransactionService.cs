using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Voting.Model;
using Voting.Model.Entities;
using Voting.Model.Exceptions;
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
        /// <param name="candidateAddress">Candidate</param>
        /// <param name="amount">Vote count (actually it should be 1)</param>
        public Transaction NewTransaction(Wallet sender, string electionAddress, string candidateAddress)
        {
            return TransactionWithOutputs(sender, candidateAddress, new List<TransactionOutput>
                {
                    new TransactionOutput(electionAddress , candidateAddress)
                }.ToArray());
        }

        private Transaction TransactionWithOutputs(Wallet sender, string recipient, params TransactionOutput[] outputs)
        {
            var transaction = new Transaction
            {
                Outputs = outputs.ToList()
            };

            SignTransaction(transaction, sender);

            return transaction;
        }

        public Transaction UpdateTransaction(Transaction transaction, Wallet sender, string electionAddress, string candidateAddress)
        {
            if (transaction.Outputs.Any(o => o.ElectionAddress == electionAddress))
            {
                Console.WriteLine("You have already voted in this election");
                return transaction;
            }

            transaction.Outputs.Add(new TransactionOutput(electionAddress, candidateAddress));

            SignTransaction(transaction, sender);

            return transaction;
        }

        public void SignTransaction(Transaction transaction, Wallet sender)
        {
            transaction.Input = new TransactionInput
            {
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

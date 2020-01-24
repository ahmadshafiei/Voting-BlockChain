using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Model;
using Voting.Model.Entities;
using Voting.Model.Exceptions;
using Voting.Infrastructure.Utility;
using Voting.Model.Context;

namespace Voting.Infrastructure.Services
{
    public class TransactionService
    {
        private readonly BlockchainContext _dbContext;

        public TransactionService(BlockchainContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// TODO: amount should change to vote (actually there should be no amount , vote is defaults to 1)
        /// Implementation change so senders balance don't decrease on sending 
        /// </summary>
        /// <param name="sender">Voter</param>
        /// <param name="candidateAddress">Candidate</param>
        public Transaction NewTransaction(Wallet sender, string electionAddress, string candidateAddress)
        {
            return TransactionWithOutputs(sender, candidateAddress, new List<TransactionOutput>
            {
                new TransactionOutput(electionAddress, candidateAddress)
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

        public Transaction UpdateTransaction(Transaction transaction, Wallet sender, string electionAddress,
            string candidateAddress)
        {
            if (transaction.Outputs.Any(o => o.ElectionAddress == electionAddress))
            {
                Console.WriteLine("You have already voted in this election");
                return transaction;
            }

            TransactionOutput output = new TransactionOutput(electionAddress, candidateAddress);

            transaction.Outputs.Add(output);

            SignTransaction(transaction, sender, true);

            return transaction;
        }

        public void SignTransaction(Transaction transaction, Wallet sender, bool isUpdating = false)
        {
            var hashedOutputs = Hash.HashTransactionOutput(transaction.Outputs.ToArray());
            var hashedoutput = Encoding.UTF8.GetString(hashedOutputs);
            var signature = Encoding.UTF8.GetString(sender.Sign(hashedOutputs));
            
            if (!isUpdating)
                transaction.Input = new TransactionInput
                {
                    Address = sender.PublicKey,
                    Signature = sender.Sign(hashedOutputs)
                };
            else
                transaction.Input.Signature = sender.Sign(hashedOutputs);
        }

        public bool VerifyTransaction(Transaction transaction)
        {
            var hashedOutputs = Hash.HashTransactionOutput(transaction.Outputs.ToArray());
            var signature = Encoding.UTF8.GetString(transaction.Input.Signature);
            var hasheoutput = Encoding.UTF8.GetString(hashedOutputs);
            
            return ECCUtility.VerifySignature(
                transaction.Input.Address,
                transaction.Input.Signature,
                hashedOutputs
            );
        }
    }
}
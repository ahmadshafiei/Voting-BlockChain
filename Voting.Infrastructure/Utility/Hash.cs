using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Voting.Model.Entities;

namespace Voting.Infrastructure.Utility
{
    public static class Hash
    {
        private readonly static HMACSHA256 sha256 = new HMACSHA256(Encoding.UTF8.GetBytes("PRIVATE_KEY"));

        public static byte[] HashBlock(this Block block)
        {
            string hashValue = $"{block.Timestamp}-{block.PreviousHash}-{block.Data}-{block.Nonce}-{block.Difficulty}";

            return HashString(hashValue);
        }

        public static byte[] HashTransactionOutput(/*bool setUTC =false ,*/ params TransactionOutput[] outputs)
        {
            List<TransactionOutput> hashedOutputs = new List<TransactionOutput>();

            foreach (TransactionOutput output in outputs)
                hashedOutputs.Add(new TransactionOutput(output.ElectionAddress, output.CandidateAddress)
                {
                    Timestamp =/*setUTC ?*/ output.Timestamp.ToUniversalTime() /*: output.Timestamp,*/
                });

            return HashString(JsonConvert.SerializeObject(hashedOutputs));
        }

        private static byte[] HashString(string data)
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        }
    }
}
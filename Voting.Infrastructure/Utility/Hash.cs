using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Votin.Model.Entities;

namespace Voting.Infrastructure.Utility
{
    public static class Hash
    {
        private static SHA256 sha256 = SHA256.Create();

        public static byte[] HashBlock(this Block block)
        {
            string hashValue = $"{block.Timestamp}-{block.PreviousHash}-{block.Data}-{block.Nonce}-{block.Difficulty}";

            return HashString(hashValue);
        }

        public static byte[] HashTransactionOutput(params TransactionOutput[] outputs)
        {
            return HashString(JsonConvert.SerializeObject(outputs));
        }

        private static byte[] HashString(string data)
        {            
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        }
    }
}

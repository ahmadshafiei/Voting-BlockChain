using System;
using System.Collections.Generic;
using System.Text;
using Votin.Model.Entities;

namespace Voting.Service.Utility
{
    public static class Hash
    {
        public static byte[] HashBlock(this Block block)
        {
            string hashValue = $"{block.Timestamp}-{block.PreviousHash}-{block.Data}";

            return HashString(hashValue);
        }

        public static byte[] HashString(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Voting.Model.Entities
{
    public class TransactionInput
    {
        public DateTime TimeStamp{ get; set; }
        public int Amount { get; set; }

        /// <summary>
        /// Hex representation of public key
        /// </summary>
        public string Address { get; set; }
        public byte[] Signature { get; set; }
    }
}

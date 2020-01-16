using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace Voting.Model.Entities
{
    public class TransactionInput
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        
        [ForeignKey(nameof(TransactionId))]
        public Transaction Transaction { get; set; }
        
        /// <summary>
        /// Hex representation of public key
        /// </summary>
        public string Address { get; set; }
        public byte[] Signature { get; set; }
    }
}

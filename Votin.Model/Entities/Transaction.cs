using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Voting.Model.Exceptions;

namespace Voting.Model.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public TransactionInput Input { get; set; }
        public List<TransactionOutput> Outputs { get; set; }
    }
}

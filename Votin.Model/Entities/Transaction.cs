using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Votin.Model.Exceptions;

namespace Votin.Model.Entities
{
    public class Transaction
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public TransactionInput Input { get; set; }
        public List<TransactionOutput> Outputs { get; set; }
    }
}

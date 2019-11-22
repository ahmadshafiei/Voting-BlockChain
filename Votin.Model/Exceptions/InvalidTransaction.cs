using System;
using System.Collections.Generic;
using System.Text;

namespace Votin.Model.Exceptions
{
    public class InvalidTransactionException : BlockChainException
    {
        public InvalidTransactionException(string message):base(message)
        {

        }
    }
}

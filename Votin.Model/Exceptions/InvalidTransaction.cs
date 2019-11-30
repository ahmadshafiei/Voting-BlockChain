using System;
using System.Collections.Generic;
using System.Text;

namespace Voting.Model.Exceptions
{
    public class InvalidTransactionException : BlockChainException
    {
        public InvalidTransactionException(string message):base(message)
        {

        }
    }
}

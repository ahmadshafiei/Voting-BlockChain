using System;
using System.Collections.Generic;
using System.Text;
using Voting.Model.Exceptions;

namespace Votin.Model.Exceptions
{
    public class NotFoundException : BlockChainException
    {
        public NotFoundException(string entity) : base($"{entity} مورد نظر یافت نشد")
        {

        }
    }
}

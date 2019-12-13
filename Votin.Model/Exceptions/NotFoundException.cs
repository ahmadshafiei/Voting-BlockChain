using System;
using System.Collections.Generic;
using System.Text;

namespace Votin.Model.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string entity) : base($"{entity} مورد نظر یافت نشد")
        {

        }
    }
}

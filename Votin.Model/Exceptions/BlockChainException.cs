using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Votin.Model.Exceptions
{
    public class BlockChainException : Exception
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;
        public string ContentType { get; set; } = "application/json";

        public BlockChainException(string message) : base(message) { }

        public BlockChainException(HttpStatusCode statusCode)
        {
            this.StatusCode = statusCode;
        }

        public BlockChainException(HttpStatusCode statusCode, string message) : base(message)
        {
            this.StatusCode = statusCode;
        }

        public BlockChainException(HttpStatusCode statusCode, Exception inner) : this(statusCode, inner.ToString()) { }
    }
}

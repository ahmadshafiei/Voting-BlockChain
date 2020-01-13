using System;
using System.Net;

namespace Voting.Model.Exceptions
{
    public class BlockChainException : Exception
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;
        public string ContentType { get; set; } = "application/json";
        
        public bool ShowError { get; set; } = true;
        public BlockChainException(string message) : base(message)
        {
        }

        public BlockChainException(HttpStatusCode statusCode)
        {
            this.StatusCode = statusCode;
        }

        public BlockChainException(HttpStatusCode statusCode, string message) : base(message)
        {
            this.StatusCode = statusCode;
        }

        public BlockChainException(HttpStatusCode statusCode, Exception inner) : this(statusCode, inner.ToString())
        {
        }
    }
}
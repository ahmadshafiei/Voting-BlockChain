namespace Voting.Model.Exceptions
{
    public class ValidationException : BlockChainException
    {
        public ValidationException(string msg):base(msg)
        {
            
        }
    }
}
namespace Voting.Model.Exceptions
{
    public class UnauthorizedException : BlockChainException
    {
        public UnauthorizedException(string msg) : base(msg)
        {
        }
    }
}
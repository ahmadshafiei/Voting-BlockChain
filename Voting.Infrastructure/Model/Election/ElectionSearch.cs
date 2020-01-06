using Voting.Infrastructure.Model.Common;

namespace Voting.Infrastructure.Model.Election
{
    public class ElectionSearch : Pagination
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
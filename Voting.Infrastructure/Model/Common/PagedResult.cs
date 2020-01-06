using System.Collections.Generic;

namespace Voting.Infrastructure.Model.Common
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
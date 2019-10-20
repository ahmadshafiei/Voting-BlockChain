using System;
using System.Collections.Generic;
using Votin.Model.Entities;

namespace Voting.Infrastructure
{
    public class BlockChain
    {
        public List<Block> Chain { get; set; } = new List<Block>();
    }
}

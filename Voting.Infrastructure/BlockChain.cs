using System;
using System.Collections.Generic;
using System.Linq;
using Votin.Model.Entities;
using Voting.Infrastructure.Utility;

namespace Voting.Infrastructure
{
    public class BlockChain
    {
        public static List<Block> Chain { get; set; } = new List<Block>();

        static BlockChain()
        {
            if (!Chain.Any())
                Chain.Add(GenesisBlock());
        }

        public BlockChain()
        {
            if (!Chain.Any())
                Chain.Add(GenesisBlock());
        }

        /// <summary>
        /// First block in chain
        /// </summary>
        /// <returns>Returns first block of chain (GENESIS)</returns>
        public static Block GenesisBlock()
        {
            Block genesis = new Block
            {
                Timestamp = DateTime.MinValue.Ticks,
                Data = null,
                PreviousHash = null,
                Nonce = 0
            };

            genesis.Hash = Hash.HashBlock(genesis);

            return genesis;
        }
    }
}

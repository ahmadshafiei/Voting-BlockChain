using System;
using System.Collections.Generic;
using System.Linq;
using Voting.Model;
using Voting.Model.Entities;
using Voting.Infrastructure.Utility;
using Voting.Model.Context;

namespace Voting.Infrastructure
{
    public class BlockChain
    {
        public static List<Block> Chain { get; set; } = new List<Block>();

        /// <summary>
        /// First block in chain
        /// </summary>
        /// <returns>Returns first block of chain (GENESIS)</returns>
        public static Block GenesisBlock()
        {
            //genesis should be same thing across all peers otherwise we get exception on synchronization
            Block genesis = new Block
            {
                Timestamp = DateTime.MinValue.Ticks,
                Data = "[]",
                PreviousHash = null,
                Nonce = 0,
                Difficulty = Config.DIFFICULTY
            };

            genesis.Hash = Hash.HashBlock(genesis);

            return genesis;
        }
    }
}
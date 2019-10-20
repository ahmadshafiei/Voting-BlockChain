using System;
using System.Collections.Generic;
using System.Text;
using Votin.Model.Entities;
using Voting.Service.Utility;

namespace Voting.Service.Services.BlockServices
{
    public class BlockService
    {
        public List<Block> Chain { get; set; } = new List<Block>();
        /// <summary>
        /// First block in chain
        /// </summary>
        /// <returns>Returns first block of chain (GENESIS)</returns>
        public Block GenesisBlock()
        {
            Block genesis = new Block
            {
                Timestamp = DateTime.MinValue.Ticks,
                Data = null,
                PreviousHash = null
            };

            genesis.Hash = Hash.HashBlock(genesis);

            return genesis;
        }


        /// <summary>
        /// Set's the block according to <paramref name="previousBlock"/> 
        /// </summary>
        /// <param name="previousBlock">Previous block in chain</param>
        /// <returns>Newly added block</returns>
        public Block MineBlock(Block previousBlock, string data)
        {
            Block block = new Block
            {
                Timestamp = DateTime.Now.Ticks,
                PreviousHash = previousBlock.Hash,
                Data = data
            };

            block.Hash = Hash.HashBlock(block);

            return block;
        }
    }
}

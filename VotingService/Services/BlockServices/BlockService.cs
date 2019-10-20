using System;
using System.Collections.Generic;
using System.Text;
using Votin.Model.Entities;
using Voting.Infrastructure.Utility;

namespace Voting.Service.Services.BlockServices
{
    public class BlockService
    {
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Votin.Model.Entities;
using Voting.Infrastructure.Utility;

namespace Voting.Infrastructure.Services.BlockServices
{
    public class BlockService
    {
        const int DIFFICULTY = 2;
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
                Data = data,
                Nonce = 0
            };

            byte[] difficulty = new byte[DIFFICULTY];

            do
            {
                block.Timestamp = DateTime.Now.Ticks;
                block.Nonce++;
                block.Hash = Hash.HashBlock(block);
            } while (!block.Hash.ToList().Take(DIFFICULTY).SequenceEqual(difficulty));

            return block;
        }
    }
}

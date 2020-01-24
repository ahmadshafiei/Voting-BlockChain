using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Voting.Model;
using Voting.Model.Entities;
using Voting.Infrastructure.Utility;

namespace Voting.Infrastructure.Services.BlockServices
{
    public class BlockService
    {
        const int DIFFICULTY = 2;
        const int MINE_RATE = 3 * 10 ^ 7; //3 Seconds in ticks

        /// <summary>
        /// Set's the block according to <paramref name="previousBlock"/> 
        /// </summary>
        /// <param name="previousBlock">Previous block in chain</param>
        /// <returns>Newly added block</returns>
        public Block MineBlock(Block previousBlock, List<Transaction> data)
        {
            string d = JsonConvert.SerializeObject(data,
                new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
                
            Block block = new Block
            {
                Timestamp = DateTime.Now.Ticks,
                PreviousHash = previousBlock.Hash,
                Data =  d,
                Nonce = 0,
                Difficulty = previousBlock.Difficulty
            };

            do
            {
                block.Timestamp = DateTime.Now.Ticks;
                block.Nonce++;
                block.Difficulty = AdjustDifficulty(previousBlock, block.Timestamp);
                block.Hash = Hash.HashBlock(block);
            } while (!block.Hash.ToList().Take(block.Difficulty).SequenceEqual(new byte[block.Difficulty]));

            return block;
        }

        private int AdjustDifficulty(Block previousBlock, long timestamp)
        {
            int difficulty = previousBlock.Difficulty;

            return previousBlock.Timestamp + Config.MINE_RATE > timestamp ? difficulty + 1 : difficulty - 1;
        }
    }
}

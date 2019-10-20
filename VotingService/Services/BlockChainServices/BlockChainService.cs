using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Votin.Model.Entities;
using Voting.Infrastructure;
using Voting.Infrastructure.Utility;
using Voting.Service.Services.BlockServices;

namespace Voting.Service.Services.BlockChainServices
{
    public class BlockChainService
    {
        private readonly BlockService _blockService;

        public BlockChainService(BlockService blockService)
        {
            _blockService = blockService;
        }

        public Block AddBlock(string data)
        {
            Block lastBlock = BlockChain.Chain.Last();

            Block block = _blockService.MineBlock(lastBlock, data);

            BlockChain.Chain.Add(block);

            return block;
        }

        public bool IsValidChain(List<Block> chain)
        {
            if (chain.First() != BlockChain.GenesisBlock())
                return false;

            for (int i = 1; i < chain.Count; i++)
            {
                Block block = chain[i];
                Block previousBlock = chain[i - 1];

                if (block.PreviousHash != previousBlock.Hash || block.Hash != Hash.HashBlock(block))
                    return false;
            }

            return true;
        }

        public void ReplaceChain(List<Block> newChain)
        {
            if (newChain.Count <= BlockChain.Chain.Count)
                throw new Exception("Invalid New Chain Length");
            else if (IsValidChain(newChain))
                throw new Exception("Invalid New Chain");

            BlockChain.Chain = newChain;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Votin.Model.Entities;
using Voting.Infrastructure;
using Voting.Service.Services.BlockServices;
using Voting.Service.Utility;

namespace Voting.Service.Services.BlockChainServices
{
    public class BlockChainService
    {
        private readonly BlockService _blockService;

        public BlockChainService(BlockService blockService)
        {
            _blockService = blockService;
            if (!_blockService.Chain.Any())
                _blockService.Chain.Add(blockService.GenesisBlock());
        }

        public Block AddBlock(string data)
        {
            Block lastBlock = _blockService.Chain.Last();

            Block block = _blockService.MineBlock(lastBlock, data);

            _blockService.Chain.Add(block);

            return block;
        }

        public bool IsValidChain(List<Block> chain)
        {
            if (chain.First() != _blockService.GenesisBlock())
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
            if (newChain.Count <= _blockService.Chain.Count)
                throw new Exception("Invalid New Chain Length");
            else if (IsValidChain(newChain))
                throw new Exception("Invalid New Chain");

            _blockService.Chain = newChain;
        }
    }
}

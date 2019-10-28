using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Votin.Model.Entities;
using Voting.Infrastructure;
using Voting.Infrastructure.Utility;
using Voting.Infrastructure.Services.BlockServices;
using Voting.Infrastructure.PeerToPeer;
using Microsoft.Extensions.DependencyInjection;

namespace Voting.Infrastructure.Services.BlockChainServices
{
    public class BlockChainService
    {
        private readonly BlockService _blockService;
        private P2PNetwork _p2PNetwork;
        private readonly IServiceProvider _serviceProvider;

        public BlockChainService(BlockService blockService, IServiceProvider serviceProvider)
        {
            _blockService = blockService;
            _serviceProvider = serviceProvider;
        }

        public List<Block> AddBlock(string data)
        {
            Block lastBlock = BlockChain.Chain.Last();

            Block block = _blockService.MineBlock(lastBlock, data);

            BlockChain.Chain.Add(block);

            //Can't use Constructor DI Because of circular injection
            _p2PNetwork = _serviceProvider.GetService<P2PNetwork>();
            _p2PNetwork.SyncChains();

            return BlockChain.Chain;
        }

        public bool IsValidChain(List<Block> chain)
        {
            if (!chain.First().Equals(BlockChain.GenesisBlock()))
                return false;

            for (int i = 1; i < chain.Count; i++)
            {
                Block block = chain[i];
                Block previousBlock = chain[i - 1];
                
                if (!block.PreviousHash.SequenceEqual(previousBlock.Hash) || !block.Hash.SequenceEqual(Hash.HashBlock(block)))
                    return false;
            }

            return true;
        }

        public void ReplaceChain(List<Block> newChain)
        {
            if (newChain.Count < BlockChain.Chain.Count)
                throw new Exception("Invalid New Chain Length");
            else if (!IsValidChain(newChain))
                throw new Exception("Invalid New Chain");

            BlockChain.Chain = newChain;
        }
    }
}

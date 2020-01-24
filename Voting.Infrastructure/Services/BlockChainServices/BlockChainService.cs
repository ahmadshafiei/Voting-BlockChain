using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Model.Entities;
using Voting.Infrastructure;
using Voting.Infrastructure.Utility;
using Voting.Infrastructure.Services.BlockServices;
using Voting.Infrastructure.PeerToPeer;
using Microsoft.Extensions.DependencyInjection;
using Voting.Model.Context;

namespace Voting.Infrastructure.Services.BlockChainServices
{
    public class BlockChainService
    {
        private readonly BlockService _blockService;
        private P2PNetwork _p2PNetwork;
        private readonly IServiceProvider _serviceProvider;
        private readonly BlockchainContext _dbContext;

        public BlockChainService(BlockService blockService, IServiceProvider serviceProvider,
            BlockchainContext dbContext)
        {
            _blockService = blockService;
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
        }

        public async Task<Block> AddBlock(List<Transaction> data)
        {
            Block lastBlock = await _dbContext.Blocks.LastAsync();

            Block block = _blockService.MineBlock(lastBlock, data);

            _dbContext.Blocks.Add(block);
            await _dbContext.SaveChangesAsync();

            //Can't use Constructor DI Because of circular injection
            _p2PNetwork = _serviceProvider.GetService<P2PNetwork>();
            _p2PNetwork.SyncChains();

            return block;
        }

        public bool IsValidChain(List<Block> chain)
        {
            if (!chain.First().Equals(BlockChain.GenesisBlock()))
                return false;

            for (int i = 1; i < chain.Count; i++)
            {
                Block block = chain[i];
                Block previousBlock = chain[i - 1];

                if (!block.PreviousHash.SequenceEqual(previousBlock.Hash) ||
                    !block.Hash.SequenceEqual(Hash.HashBlock(block)))
                    return false;
            }

            return true;
        }

        public void ReplaceChain(List<Block> newChain)
        {
            var blockCount = _dbContext.Blocks.Count();

            if (newChain.Count < blockCount)
            {
                Console.WriteLine("Invalid New Chain Length");
                return;
            }

            else if (!IsValidChain(newChain))
            {
                Console.WriteLine("Invalid New Chain");
                return;
            }
            
            _dbContext.Blocks.RemoveRange();
            _dbContext.Blocks.AddRange(newChain);
            _dbContext.SaveChanges();
        }
    }
}
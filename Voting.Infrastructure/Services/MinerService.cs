using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voting.Model.Entities;
using Voting.Infrastructure.PeerToPeer;
using Voting.Infrastructure.Services.BlockChainServices;

namespace Voting.Infrastructure.Services
{
    public class MinerService
    {
        private readonly BlockChainService _blockChainService;
        private readonly TransactionPoolService _transactionPoolService;
        private readonly TransactionService _transactionService;
        private readonly WalletService _walletService;
        private readonly P2PNetwork _p2pNetwork;

        public MinerService(BlockChainService blockChainService,
            TransactionPoolService transactionPoolService,
            WalletService walletService,
            TransactionService transactionService,
            P2PNetwork p2PNetwork)
        {
            _blockChainService = blockChainService;
            _transactionPoolService = transactionPoolService;
            _transactionService = transactionService;
            _walletService = walletService;
            _p2pNetwork = p2PNetwork;
        }

        public async Task<Block> Mine()
        {
            List<Transaction> validTransactions = await _transactionPoolService.GetValidTransactions();

            Block block = null;

            if (validTransactions.Any())
            {
                block = await _blockChainService.AddBlock(validTransactions);

                await _transactionPoolService.ClearPool();

                _p2pNetwork.BroadcastClearTransactionPool();
            }

            return block;
        }
        
        
        
    }
}
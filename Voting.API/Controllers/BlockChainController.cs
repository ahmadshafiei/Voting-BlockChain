using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using Votin.Model.API.BlockChain;
using Votin.Model.Entities;
using Voting.Infrastructure;
using Voting.Infrastructure.Services;
using Voting.Infrastructure.Services.BlockChainServices;
using Voting.Infrastructure.Services.BlockServices;
using Nethereum.Signer;
using Nethereum.Signer.Crypto;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using System.Text;
using Voting.Infrastructure.PeerToPeer;

namespace Voting.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BlockChainController : ControllerBase
    {
        private readonly BlockService _blockService;
        private readonly BlockChainService _blockChainService;
        private readonly TransactionPoolService _transactionPoolService;
        private readonly WalletService _walletService;
        private readonly P2PNetwork _p2pNetwork;
        private readonly Wallet _wallet;

        public BlockChainController(BlockChainService blockChainService, BlockService blockService, TransactionPoolService transactionPoolService, WalletService walletService, P2PNetwork p2pNetwork)
        {
            _blockService = blockService;
            _blockChainService = blockChainService;
            _transactionPoolService = transactionPoolService;
            _walletService = walletService;
            _p2pNetwork = p2pNetwork;
            _wallet = new Wallet();
        }

        [HttpGet]
        public IActionResult GetBlockChain()
        {
            return Ok(BlockChain.Chain);
        }

        [HttpPost]
        public IActionResult MineBlock(BlockData data)
        {
            return Ok(_blockChainService.AddBlock(data.Data));
        }

        [HttpGet]
        public IActionResult GetTransactions()
        {
            return Ok(_transactionPoolService.Transactions);
        }


        [HttpPost]
        public IActionResult AddTransaction(TransactionData transaction)
        {
            Votin.Model.Entities.Transaction t = _walletService.CreateTransaction(_wallet, transaction.Recipient, transaction.Amount, _transactionPoolService);

            _p2pNetwork.BroadcastTransaction(t);

            return Ok(_transactionPoolService.Transactions);
        }
    }
}

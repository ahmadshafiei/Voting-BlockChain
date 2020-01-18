using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using Voting.Model.API.BlockChain;
using Voting.Model.Entities;
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
using System.Collections.Generic;
using Newtonsoft.Json;

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
        private Wallet _wallet;
        private readonly MinerService _minerService;

        public BlockChainController(BlockChainService blockChainService, BlockService blockService, TransactionPoolService transactionPoolService, WalletService walletService, P2PNetwork p2pNetwork
            , MinerService minerService)
        {
            _blockService = blockService;
            _blockChainService = blockChainService;
            _transactionPoolService = transactionPoolService;
            _walletService = walletService;
            _p2pNetwork = p2pNetwork;
            _minerService = minerService;
            InitWallet();
        }

        private void InitWallet()
        {
            string key = HttpContext.Request.Headers["WalletKey"];
            _wallet = new Wallet(key);
        }

        [HttpGet]
        public IActionResult GetBlockChain()
        {
            return Ok(BlockChain.Chain);
        }
        //
        //
        // [HttpGet]
        // public IActionResult GetTransactions()
        // {
        //     return Ok(_transactionPoolService.Transactions);
        // }
        //
        [HttpGet]
        public IActionResult GetPublicKey()
        {
            return Ok(_wallet.PublicKey);
        }
        //
        // [HttpGet]
        // public IActionResult MineTransaction()
        // {
        //     Block block = _minerService.Mine(_wallet);
        //
        //     Console.WriteLine(JsonConvert.SerializeObject(block));
        //
        //     return GetBlockChain();
        // }
    }
}

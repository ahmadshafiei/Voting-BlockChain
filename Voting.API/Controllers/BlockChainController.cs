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
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Voting.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BlockChainController : ControllerBase
    {
        private readonly MinerService _minerService;

        public BlockChainController(MinerService minerService)
        {
            _minerService = minerService;
        }
        
        [HttpGet]
        public async Task<IActionResult> MineTransaction()
        {
            Block block = await _minerService.Mine();

            return Ok(block);
        }
    }
}
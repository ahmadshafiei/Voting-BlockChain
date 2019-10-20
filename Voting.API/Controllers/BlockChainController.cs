using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Votin.Model.API.BlockChain;
using Votin.Model.Entities;
using Voting.Infrastructure;
using Voting.Service.Services.BlockChainServices;
using Voting.Service.Services.BlockServices;

namespace Voting.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BlockChainController : ControllerBase
    {
        private BlockService _blockService;
        private BlockChainService _blockChainService;

        public BlockChainController(BlockChainService blockChainService , BlockService blockService)
        {
            _blockService = blockService;
            _blockChainService = blockChainService;
        }

        [HttpGet]
        public IActionResult GetBlockChain()
        {
            return Ok(_blockService.Chain);
        }

        [HttpPost]
        public IActionResult MineBlock(BlockData data)
        {
            return Ok(_blockChainService.AddBlock(data.Data));
        }
    }
}

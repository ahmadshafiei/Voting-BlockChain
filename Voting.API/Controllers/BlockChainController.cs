using Microsoft.AspNetCore.Mvc;
using Votin.Model.API.BlockChain;
using Voting.Infrastructure;
using Voting.Infrastructure.Services.BlockChainServices;
using Voting.Infrastructure.Services.BlockServices;

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
            return Ok(BlockChain.Chain);
        }

        [HttpPost]
        public IActionResult MineBlock(BlockData data)
        {
            return Ok(_blockChainService.AddBlock(data.Data));
        }
    }
}

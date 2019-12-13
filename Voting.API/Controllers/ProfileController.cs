using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Voting.Infrastructure.Services;
using Voting.Model.Entities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Voting.API.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ProfileController : Controller
    {
        private readonly ProfileService _profileService;
        private readonly WalletService _walletService;

        public ProfileController(
                ProfileService profileService,
                WalletService walletService
            )
        {
            _walletService = walletService;
        }

        [HttpGet]
        public IActionResult GetNewWallet()
        {
            return Ok(_profileService.GetNewWallet());
        }

        [HttpGet]
        public IActionResult GetPublicKey(string privateKey)
        {
            return Ok(_profileService.GetPublicKey(privateKey));
        }

    }
}

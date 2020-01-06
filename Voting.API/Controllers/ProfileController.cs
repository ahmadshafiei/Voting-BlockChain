using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Voting.Infrastructure.Services;
using Voting.Model.Entities;

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
            _profileService = profileService;
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
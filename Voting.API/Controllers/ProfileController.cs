using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Voting.Infrastructure.API.User;
using Voting.Infrastructure.Model.Profile;
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

        [HttpPost]
        public async Task<IActionResult> GetNewWallet([FromBody] CreateWalletModel model)
        {
            var wallet = await _profileService.GetNewWallet(model);
            return Ok(wallet);
        }

        [HttpGet]
        public async Task<IActionResult> GetPublicKey(string privateKey)
        {
            var result = await _profileService.GetPublicKey(privateKey);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserSearch filter)
        {
            var result = await _profileService.GetUsersAsync(filter);
            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> IsAdmin(string publicKey)
        {
            var isAdmin = await _profileService.IsAdminAsync(publicKey);
            return Ok(isAdmin);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsername(string publicKey)
        {
            var user = await _profileService.GetUsernameAsync(publicKey);
            return Ok(user);
        }
    }
}
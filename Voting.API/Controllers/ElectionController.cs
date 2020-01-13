using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Voting.Model.Entities;
using Voting.Infrastructure.DTO.Election;
using Voting.Infrastructure.Model.Common;
using Voting.Infrastructure.Model.Election;
using Voting.Infrastructure.Services;

namespace Voting.API.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ElectionController : Controller
    {
        private readonly ElectionService _electionService;

        public ElectionController(ElectionService electionService)
        {
            _electionService = electionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetElections([FromQuery] ElectionSearch model)
        {
            PagedResult<ElectionDTO> elections = await _electionService.GetElectionsAsync(model);

            return Ok(elections);
        }

        [HttpGet]
        public async Task<IActionResult> GetElection(int electionId)
        {
            Election election = await _electionService.GetElectionAsync(electionId);

            return Ok(election);
        }

        [HttpPost]
        public async Task<IActionResult> CreateElection([FromBody] Election election)
        {
            await _electionService.CreateElectionAsync(election);

            return Ok();
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateElection(Election election)
        {
            await _electionService.UpdateElectionAsync(election);

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveElection(int electionId)
        {
            await _electionService.RemoveElectionAsync(electionId);

            return Ok();
        }
    }
}
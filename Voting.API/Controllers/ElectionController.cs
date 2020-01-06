using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Votin.Model.Entities;
using Voting.Infrastructure.DTO.Election;
using Voting.Infrastructure.Model.Common;
using Voting.Infrastructure.Model.Election;
using Voting.Infrastructure.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
        public IActionResult GetElections([FromQuery] ElectionSearch model)
        {
            PagedResult<ElectionDTO> elections = _electionService.GetElections(model);

            return Ok(elections);
        }

        [HttpGet]
        public IActionResult GetElection(string address)
        {
            Election election = _electionService.GetElection(address);

            return Ok(election);
        }

        [HttpPost]
        public IActionResult CreateElection([FromBody] Election election)
        {
            _electionService.CreateElection(election);

            return Ok();
        }

        [HttpPatch]
        public IActionResult UpdateElection(Election election)
        {
            _electionService.UpdateElection(election);

            return Ok();
        }

        [HttpDelete]
        public IActionResult RemoveElection(string address)
        {
            _electionService.RemoveElection(address);

            return Ok();
        }
    }
}
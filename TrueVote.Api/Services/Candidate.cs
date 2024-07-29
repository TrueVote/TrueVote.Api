using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;
using TrueVote.Api.Helpers;

namespace TrueVote.Api.Services
{
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status409Conflict)]
    public class Candidate : ControllerBase
    {
        private readonly ILogger _log;
        private readonly ITrueVoteDbContext _trueVoteDbContext;
        private readonly IServiceBus _serviceBus;

        public Candidate(ILogger log, ITrueVoteDbContext trueVoteDbContext, IServiceBus serviceBus)
        {
            _log = log;
            _trueVoteDbContext = trueVoteDbContext;
            _serviceBus = serviceBus;
        }

        [HttpPost]
        [Route("candidate")]
        [Produces(typeof(CandidateModel))]
        [Description("Returns the added Candidate")]
        [ProducesResponseType(typeof(CandidateModel), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateCandidate([FromBody] BaseCandidateModel baseCandidate)
        {
            _log.LogDebug("HTTP trigger - CreateCandidate:Begin");

            _log.LogInformation($"Request Data: {baseCandidate}");

            var candidate = new CandidateModel { CandidateId = Guid.NewGuid().ToString(), Name = baseCandidate.Name, PartyAffiliation = baseCandidate.PartyAffiliation, DateCreated = UtcNowProviderFactory.GetProvider().UtcNow, Selected = false };

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Candidates.AddAsync(candidate);
            await _trueVoteDbContext.SaveChangesAsync();

            await _serviceBus.SendAsync($"New TrueVote Candidate created: {baseCandidate.Name}");

            _log.LogDebug("HTTP trigger - CreateCandidate:End");

            return CreatedAtAction(null, null, candidate);
        }

        [HttpGet]
        [Route("candidate/find")]
        [Produces(typeof(CandidateModelList))]
        [Description("Returns collection of Candidates")]
        [ProducesResponseType(typeof(CandidateModelList), StatusCodes.Status200OK)]
        public async Task<IActionResult> CandidateFind([FromBody] FindCandidateModel findCandidate)
        {
            _log.LogDebug("HTTP trigger - CandidateFind:Begin");

            _log.LogInformation($"Request Data: {findCandidate}");

            var items = new CandidateModelList
            {
                Candidates = await _trueVoteDbContext.Candidates
                .Where(c =>
                    (findCandidate.Name == null || (c.Name ?? string.Empty).ToLower().Contains(findCandidate.Name.ToLower())) &&
                    (findCandidate.PartyAffiliation == null || (c.PartyAffiliation ?? string.Empty).ToLower().Contains(findCandidate.PartyAffiliation.ToLower())))
                .OrderByDescending(c => c.DateCreated).ToListAsync()
            };

            _log.LogDebug("HTTP trigger - CandidateFind:End");

            return items.Candidates.Count == 0 ? NotFound() : Ok(items);
        }
    }
}

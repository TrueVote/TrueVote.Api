using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrueVote.Api.Helpers;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;

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
    public class Ballot : ControllerBase
    {
        private readonly ILogger _log;
        private readonly ITrueVoteDbContext _trueVoteDbContext;
        private readonly IValidator _validator;
        private readonly IServiceBus _serviceBus;

        public Ballot(ILogger log, ITrueVoteDbContext trueVoteDbContext, IValidator validator, IServiceBus serviceBus)
        {
            _log = log;
            _trueVoteDbContext = trueVoteDbContext;
            _validator = validator;
            _serviceBus = serviceBus;
        }

        [HttpPost]
        [Route("ballot/submitballot")]
        [Produces(typeof(SubmitBallotModelResponse))]
        [Description("Election Model with vote selections")]
        [ProducesResponseType(typeof(SubmitBallotModelResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> SubmitBallot([FromBody] SubmitBallotModel bindSubmitBallotModel)
        {
            _log.LogDebug("HTTP trigger - SubmitBallot:Begin");

            _log.LogInformation("Request Data: {Model}", bindSubmitBallotModel);

            // TODO Validate the ballot
            // 1. Must have a UserId and not have already submitted a ballot for this election
            // 2. Confirm the election id exists
            // 3. Confirm the election data for this ballot has not been altered.
            // ADD CODE FOR ABOVE ITEMS HERE

            var ballot = new BallotModel { Election = bindSubmitBallotModel.Election, BallotId = Guid.NewGuid().ToString(), DateCreated = UtcNowProviderFactory.GetProvider().UtcNow };
            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Ballots.AddAsync(ballot);
            await _trueVoteDbContext.SaveChangesAsync();

            // TODO Localize .Message
            var submitBallotResponse = new SubmitBallotModelResponse
            {
                ElectionId = bindSubmitBallotModel.Election.ElectionId,
                BallotId = ballot.BallotId,
                Message = $"Ballot successfully submitted. Election ID: {bindSubmitBallotModel.Election.ElectionId}, Ballot ID: {ballot.BallotId}"
            };

            // Post a message to Service Bus for this Ballot
            await _serviceBus.SendAsync($"New TrueVote Ballot successfully submitted. Election ID: {bindSubmitBallotModel.Election.ElectionId}, Ballot ID: {ballot.BallotId}");

            // FOR NOW ONLY - THIS LINE SHOULD BE REPLACED WITH A POST TO SERVICE BUS
            // Hash the ballot
            try
            {
                await _validator.HashBallotAsync(ballot);
            }
            catch (Exception e)
            {
                _log.LogError("HashBallotAsync()");
                _log.LogDebug("HTTP trigger - SubmitBallot:End");

                var msg = submitBallotResponse.Message += " - Failure Hashing: " + e.Message;

                return Conflict(new SecureString { Value = msg });
            }

            _log.LogDebug("HTTP trigger - SubmitBallot:End");

            // TODO Return a Ballot Submitted model response with critical key data to bind ballot / user
            return CreatedAtAction(null, null, submitBallotResponse);
        }

        [HttpGet]
        [Route("ballot/find")]
        [Produces(typeof(BallotList))]
        [Description("Returns collection of Ballots")]
        [ProducesResponseType(typeof(BallotList), StatusCodes.Status200OK)]
        public async Task<IActionResult> BallotFind([FromBody] FindBallotModel findBallot)
        {
            _log.LogDebug("HTTP trigger - BallotFind:Begin");

            _log.LogInformation($"Request Data: {findBallot}");

            var items = new BallotList
            {
                Ballots = await _trueVoteDbContext.Ballots
                .Where(e =>
                    findBallot.BallotId == null || (e.BallotId ?? string.Empty).ToLower().Contains(findBallot.BallotId.ToLower()))
                .OrderByDescending(e => e.DateCreated).ToListAsync(),
                BallotHashes = await _trueVoteDbContext.BallotHashes
                .Where(e =>
                    findBallot.BallotId == null || (e.BallotId ?? string.Empty).ToLower().Contains(findBallot.BallotId.ToLower()))
                .OrderByDescending(e => e.DateCreated).ToListAsync()
            };

            _log.LogDebug("HTTP trigger - BallotFind:End");

            return items.Ballots.Count == 0 ? NotFound() : Ok(items);
        }

        [HttpGet]
        [Route("ballot/count")]
        [Produces(typeof(CountBallotModelResponse))]
        [Description("Returns count of Ballots")]
        [ProducesResponseType(typeof(CountBallotModelResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> BallotCount([FromBody] CountBallotModel countBallot)
        {
            _log.LogDebug("HTTP trigger - BallotCount:Begin");

            _log.LogInformation($"Request Data: {countBallot}");

            var items = await _trueVoteDbContext.Ballots
                .Where(c => c.DateCreated >= countBallot.DateCreatedStart && c.DateCreated <= countBallot.DateCreatedEnd)
                .OrderByDescending(c => c.DateCreated).ToListAsync();

            var ballotCountModelResponse = new CountBallotModelResponse { BallotCount = items.Count };

            _log.LogDebug("HTTP trigger - BallotCount:End");

            return Ok(ballotCountModelResponse);
        }

        [HttpGet]
        [Route("ballot/findhash")]
        [Produces(typeof(List<BallotHashModel>))]
        [Description("Returns collection of Ballot Hashes")]
        [ProducesResponseType(typeof(List<BallotHashModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BallotHashFind([FromBody] FindBallotHashModel findBallotHash)
        {
            _log.LogDebug("HTTP trigger - BallotHashFind:Begin");

            _log.LogInformation($"Request Data: {findBallotHash}");

            var items = await _trueVoteDbContext.BallotHashes
                .Where(e =>
                    findBallotHash.BallotId == null || (e.BallotId ?? string.Empty).ToLower().Contains(findBallotHash.BallotId.ToLower()))
                .OrderByDescending(e => e.DateCreated).ToListAsync();

            _log.LogDebug("HTTP trigger - BallotHashFind:End");

            return items.Count == 0 ? NotFound() : Ok(items);
        }
    }
}

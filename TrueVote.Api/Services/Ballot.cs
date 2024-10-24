using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IServiceBus _serviceBus;
        private readonly IRecursiveValidator _recursiveValidator;
        private readonly ITopicEventSender _eventSender;
        private readonly Query _query;

        public Ballot(ILogger log, ITrueVoteDbContext trueVoteDbContext, IServiceBus serviceBus, IRecursiveValidator recursiveValidator, ITopicEventSender eventSender, Query query)
        {
            _log = log;
            _trueVoteDbContext = trueVoteDbContext;
            _serviceBus = serviceBus;
            _recursiveValidator = recursiveValidator;
            _eventSender = eventSender;
            _query = query;
        }

        [HttpPost]
        [Authorize]
        [RequireRole(UserRoles.Voter_Role)]
        [ServiceFilter(typeof(ValidateUserIdFilter))]
        [Route("ballot/submitballot")]
        [Produces(typeof(SubmitBallotModelResponse))]
        [Description("Election Model with vote selections")]
        [ProducesResponseType(typeof(SubmitBallotModelResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> SubmitBallot([FromBody] SubmitBallotModel bindSubmitBallotModel)
        {
            _log.LogDebug("HTTP trigger - SubmitBallot:Begin");

            _log.LogInformation($"Request Data: {bindSubmitBallotModel}");

            // TODO Validate the ballot
            // 1. Must have a UserId and not have already submitted a ballot for this election
            // 2. Confirm the election id exists - DONE
            // 3. Confirm the election data for this ballot has not been altered. - DONE
            // 4. Confirm none of the races have null for 'Selected'. Must be true or false. - DONE
            // 5. Confirm ballot is within election time range - DONE
            // ADD CODE FOR ABOVE ITEMS HERE
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(bindSubmitBallotModel);
            validationContext.Items["IsBallot"] = true; // TODO https://truevote.atlassian.net/browse/AD-113
            validationContext.Items["DBContext"] = _trueVoteDbContext;
            validationContext.Items["Logger"] = _log;
            if (!_recursiveValidator.TryValidateObjectRecursive(bindSubmitBallotModel, validationContext, validationResults))
            {
                var errorDictionary = _recursiveValidator.GetValidationErrorsDictionary(validationResults);

                return ValidationProblem(new ValidationProblemDetails(errorDictionary));
            }

            // Get the GUID UserId from the JWT
            var userId = User.FindAll(ClaimTypes.NameIdentifier).FirstOrDefault(u => !u.Value.StartsWith("npub"))?.Value;
            if (userId == null)
            {
                _log.LogDebug("HTTP trigger - SubmitBallot:End");
                return NotFound(new SecureString { Value = $"UserId not found" });
            }

            // Check if user already submitted ballot for this election
            var alreadySubmitted = await _trueVoteDbContext.ElectionUserBindings.Where(u => u.UserId == userId && u.ElectionId == bindSubmitBallotModel.Election.ElectionId).FirstOrDefaultAsync();
            if (alreadySubmitted != null)
            {
                _log.LogDebug("HTTP trigger - SubmitBallot:End");
                return Conflict(new SecureString { Value = $"Ballot already submitted for User" });
            }

            var now = UtcNowProviderFactory.GetProvider().UtcNow;

            // Check if access code has been used or is invalid. Note timestamp only stores the .Date, not the time.
            var usedAccessCode = new UsedAccessCodeModel { AccessCode = bindSubmitBallotModel.AccessCode, DateCreated = now.Date };

            // Determine if the EAC exists
            var accessCode = await _trueVoteDbContext.ElectionAccessCodes.Where(u => u.AccessCode == usedAccessCode.AccessCode).FirstOrDefaultAsync();
            if (accessCode == null)
            {
                _log.LogDebug("HTTP trigger - SubmitBallot:End");
                return NotFound(new SecureString { Value = $"AccessCode: '{usedAccessCode.AccessCode}' not found" });
            }

            // Determine if EAC was already used
            var alreadyUsed = await _trueVoteDbContext.UsedAccessCodes.Where(u => u.AccessCode == usedAccessCode.AccessCode).FirstOrDefaultAsync();
            if (alreadyUsed != null)
            {
                _log.LogDebug("HTTP trigger - SubmitBallot:End");
                return Conflict(new SecureString { Value = $"AccessCode: '{usedAccessCode.AccessCode}' already used" });
            }

            var ballot = new BallotModel { Election = bindSubmitBallotModel.Election, ElectionId = bindSubmitBallotModel.Election.ElectionId, BallotId = Guid.NewGuid().ToString(), DateCreated = now };

            // TODO Localize .Message
            var submitBallotResponse = new SubmitBallotModelResponse
            {
                ElectionId = bindSubmitBallotModel.Election.ElectionId,
                BallotId = ballot.BallotId,
                Message = $"Election ID: {bindSubmitBallotModel.Election.ElectionId}, Ballot ID: {ballot.BallotId}"
            };

            var electionUserBindingModel = new ElectionUserBindingModel
            {
                ElectionId = bindSubmitBallotModel.Election.ElectionId,
                UserId = userId,
                DateCreated = now.Date
            };

            try
            {
                await _trueVoteDbContext.Ballots.AddAsync(ballot);
                await _trueVoteDbContext.UsedAccessCodes.AddAsync(usedAccessCode);
                await _trueVoteDbContext.ElectionUserBindings.AddAsync(electionUserBindingModel);
                await _trueVoteDbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _log.LogError("Error in DB Operation Saving Ballot");
                _log.LogDebug("HTTP trigger - SubmitBallot:End");

                var msg = submitBallotResponse.Message += " - Error in DB Operation Saving Ballot: " + e.Message;

                return Conflict(new SecureString { Value = msg });
            }

            // Post a message to Service Bus for this Ballot
            await _serviceBus.SendAsync($"New TrueVote Ballot successfully submitted. Election ID: {bindSubmitBallotModel.Election.ElectionId}, Ballot ID: {ballot.BallotId}");

            // TODO AD-137 Should optimize this to not do it on every single ballot.
            // Post a subscription event for client data refresh. This happens async to not hold up the return of the SubmitBallot() method
            await _query.GetElectionResultsByElectionId(ballot.ElectionId).ContinueWith(async task => {
                var updatedResults = await task;
                await _eventSender.SendAsync($"{nameof(Subscription.ElectionResultsUpdated)}.{updatedResults.ElectionId}", updatedResults).ConfigureAwait(false);
            }).ConfigureAwait(false);

            _log.LogDebug("HTTP trigger - SubmitBallot:End");

            submitBallotResponse.Message += " - Ballot successfully submitted.";

            // TODO Return a Ballot Submitted model response with critical key data to bind ballot / user
            return CreatedAtAction(null, null, submitBallotResponse);
        }

        [HttpGet]
        [Route("ballot/find")]
        [Produces(typeof(BallotList))]
        [Description("Returns collection of Ballots")]
        [ProducesResponseType(typeof(BallotList), StatusCodes.Status200OK)]
        public async Task<IActionResult> BallotFind([ModelBinder(BinderType = typeof(QueryStringModelBinder))] [FromQuery] FindBallotModel findBallot)
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
        public async Task<IActionResult> BallotCount([ModelBinder(BinderType = typeof(QueryStringModelBinder))] [FromQuery] CountBallotModel countBallot)
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
        public async Task<IActionResult> BallotHashFind([ModelBinder(BinderType = typeof(QueryStringModelBinder))] [FromQuery] FindBallotHashModel findBallotHash)
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

        [NonAction]
        public async Task<List<BallotModel>> GetBallotsWithoutHashesAsync(CancellationToken cancellationToken)
        {
            try
            {
                // This retrieves ALL the BallotHashes, which could be in the millions.
                // TODO Optimize this to use a CosmosDB join or some other way. Maybe have another table
                // that sets a flag and query that. Or have the flag live in the Ballots table and do updates when it's hashed
                var allBallotHashIds = await _trueVoteDbContext.BallotHashes.Select(bh => bh.BallotId)
                    .ToListAsync(cancellationToken);

                var ballotHashIdSet = new HashSet<string>(allBallotHashIds);

                var ballotsWithoutHashes = await _trueVoteDbContext.Ballots.Where(ballot => !ballotHashIdSet.Contains(ballot.BallotId))
                    .OrderByDescending(e => e.DateCreated)
                    .ToListAsync(cancellationToken);

                _log.LogDebug("Found {count} ballots without hashes", ballotsWithoutHashes.Count);

                return ballotsWithoutHashes;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while fetching ballots without hashes");
                return null;
            }
        }
    }
}

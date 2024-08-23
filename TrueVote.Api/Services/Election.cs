using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
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
    public class Election : ControllerBase
    {
        private readonly ILogger _log;
        private readonly ITrueVoteDbContext _trueVoteDbContext;
        private readonly IServiceBus _serviceBus;

        public Election(ILogger log, ITrueVoteDbContext trueVoteDbContext, IServiceBus serviceBus)
        {
            _log = log;
            _trueVoteDbContext = trueVoteDbContext;
            _serviceBus = serviceBus;
        }

        [HttpPost]
        [Route("election")]
        [Produces(typeof(ElectionModel))]
        [Description("Returns the added Election")]
        [ProducesResponseType(typeof(ElectionModel), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateElection([FromBody] BaseElectionModel baseElection)
        {
            _log.LogDebug("HTTP trigger - CreateElection:Begin");

            _log.LogInformation($"Request Data: {baseElection}");

            var races = baseElection.BaseRaces.DTOToRaces();
            var election = baseElection.DTOToElection(races);

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Elections.AddAsync(election);
            await _trueVoteDbContext.SaveChangesAsync();

            await _serviceBus.SendAsync($"New TrueVote Election created: {baseElection.Name}");

            _log.LogDebug("HTTP trigger - CreateElection:End");

            return CreatedAtAction(null, null, election);
        }

        [HttpGet]
        [Route("election/find")]
        [Produces(typeof(ElectionModelList))]
        [Description("Returns collection of Elections")]
        [ProducesResponseType(typeof(ElectionModelList), StatusCodes.Status200OK)]
        public async Task<IActionResult> ElectionFind([FromBody] FindElectionModel findElection)
        {
            _log.LogDebug("HTTP trigger - ElectionFind:Begin");

            _log.LogInformation($"Request Data: {findElection}");

            var items = new ElectionModelList
            {
                Elections = await _trueVoteDbContext.Elections
                .Where(e =>
                    findElection.Name == null || findElection.Name == "All" || (e.Name ?? string.Empty).ToLower().Contains(findElection.Name.ToLower()))
                .OrderByDescending(e => e.DateCreated).ToListAsync()
            };

            _log.LogDebug("HTTP trigger - ElectionFind:End");

            return items.Elections.Count == 0 ? NotFound() : Ok(items);
        }

        [HttpPost]
        [Route("election/addraces")]
        [Produces(typeof(ElectionModel))]
        [Description("Adds Races to an Election and returns the updated Election")]
        [ProducesResponseType(typeof(ElectionModel), StatusCodes.Status201Created)]
        public async Task<IActionResult> AddRaces([FromBody] AddRacesModel bindRaceElectionModel)
        {
            _log.LogDebug("HTTP trigger - AddRaces:Begin");

            _log.LogInformation($"Request Data: {bindRaceElectionModel}");

            // Check if the election exists. If so, return it detached from EF
            var election = await _trueVoteDbContext.Elections.Where(r => r.ElectionId == bindRaceElectionModel.ElectionId).AsNoTracking().OrderByDescending(r => r.DateCreated).FirstOrDefaultAsync();
            if (election == null)
            {
                return NotFound(new SecureString { Value = $"Election: '{bindRaceElectionModel.ElectionId}' not found" });
            }

            // Check if each Race exists or is already part of the election. If any problems, exit with error
            foreach (var rid in bindRaceElectionModel.RaceIds)
            {
                // Ensure Race exists in Race store
                var race = await _trueVoteDbContext.Races.Where(r => r.RaceId == rid).OrderByDescending(c => c.DateCreated).FirstOrDefaultAsync();
                if (race == null)
                {
                    return NotFound(new SecureString { Value = $"Race: '{rid}' not found" });
                }

                // Check if it's already part of the Election
                var raceExists = election.Races.FirstOrDefault(r => r.RaceId == rid);
                if (raceExists != null)
                {
                    return Conflict(new SecureString { Value = $"Race: '{rid}' already exists in Election" });
                }

                // Made it this far, add the Race to the Election. Ok to add here because if another one in the list, it won't get persisted
                election.Races.Add(race);
            }

            // If made through the loop of checks above, ok to persist. This will write a new Election
            election.DateCreated = UtcNowProviderFactory.GetProvider().UtcNow;
            election.ElectionId = Guid.NewGuid().ToString();

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Elections.AddAsync(election);
            await _trueVoteDbContext.SaveChangesAsync();

            _log.LogDebug("HTTP trigger - AddRaces:End");

            return CreatedAtAction(null, null, election);
        }

        [HttpPost]
        [Authorize]
        [ServiceFilter(typeof(ValidateUserIdFilter))]
        [Route("election/createaccesscodes")]
        [Produces(typeof(AccessCodesResponse))]
        [Description("Returns an AccessCodesResponse with a list of AccessCodes")]
        [ProducesResponseType(typeof(AccessCodesResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateAccessCodes([FromBody] AccessCodesRequest accessCodesRequest)
        {
            _log.LogDebug("HTTP trigger - CreateAccessCodes:Begin");

            _log.LogInformation($"Request Data: {accessCodesRequest}");

            if (User == null || User.Identity == null)
            {
                _log.LogDebug("HTTP trigger - CreateAccessCodes:End");
                return Unauthorized();
            }

            // Determine if User is found
            var foundUser = await _trueVoteDbContext.Users.Where(u => u.UserId == accessCodesRequest.UserId).FirstOrDefaultAsync();
            if (foundUser == null)
            {
                _log.LogDebug("HTTP trigger - CreateAccessCodes:End");
                return NotFound(new SecureString { Value = $"User: '{accessCodesRequest.UserId}' not found" });
            }

            // Check if the election exists.
            var election = await _trueVoteDbContext.Elections.Where(r => r.ElectionId == accessCodesRequest.ElectionId).AsNoTracking().OrderByDescending(r => r.DateCreated).FirstOrDefaultAsync();
            if (election == null)
            {
                _log.LogDebug("HTTP trigger - CreateAccessCodes:End");
                return NotFound(new SecureString { Value = $"Election: '{accessCodesRequest.ElectionId}' not found" });
            }

            var requestId = Guid.NewGuid().ToString();
            var dateCreated = UtcNowProviderFactory.GetProvider().UtcNow;

            var accessCodesResponse = new AccessCodesResponse
            {
                ElectionId = accessCodesRequest.ElectionId,
                RequestId = requestId,
                AccessCodes = []
            };

            await _trueVoteDbContext.EnsureCreatedAsync();

            for (var i = 0; i < accessCodesRequest.NumberOfAccessCodes; i++)
            {
                var uniqueKey = UniqueKeyGenerator.GenerateUniqueKey();

                var accessCode = new AccessCodeModel
                {
                    RequestId = requestId,
                    ElectionId = accessCodesRequest.ElectionId,
                    DateCreated = dateCreated,
                    AccessCode = uniqueKey,
                    RequestDescription = accessCodesRequest.RequestDescription,
                    RequestedByUserId = accessCodesRequest.UserId
                };

                accessCodesResponse.AccessCodes.Add(accessCode);
            }

            await _trueVoteDbContext.ElectionAccessCodes.AddRangeAsync(accessCodesResponse.AccessCodes);

            await _trueVoteDbContext.SaveChangesAsync();

            await _serviceBus.SendAsync($"Election Access Codes created for ElectionId: {accessCodesRequest.ElectionId}, Number of Access Codes: {accessCodesRequest.NumberOfAccessCodes}");

            _log.LogDebug("HTTP trigger - CreateAccessCodes:End");

            return CreatedAtAction(null, null, accessCodesResponse);
        }

        [HttpGet]
        [Authorize]
        [ServiceFilter(typeof(ValidateUserIdFilter))]
        [Route("election/checkaccesscode")]
        [Produces(typeof(Election))]
        [Description("Returns an Election for the AccessCode")]
        [ProducesResponseType(typeof(Election), StatusCodes.Status201Created)]
        public async Task<IActionResult> CheckAccessCode([FromBody] CheckCodeRequest checkCodeRequest)
        {
            _log.LogDebug("HTTP trigger - CheckAccessCode:Begin");

            _log.LogInformation($"Request Data: {checkCodeRequest}");

            if (User == null || User.Identity == null)
            {
                _log.LogDebug("HTTP trigger - CheckAccessCode:End");
                return Unauthorized();
            }

            // Determine if User is found
            var foundUser = await _trueVoteDbContext.Users.Where(u => u.UserId == checkCodeRequest.UserId).FirstOrDefaultAsync();
            if (foundUser == null)
            {
                _log.LogDebug("HTTP trigger - CheckAccessCode:End");
                return NotFound(new SecureString { Value = $"User: '{checkCodeRequest.UserId}' not found" });
            }

            // Determine if the EAC exists
            var accessCode = await _trueVoteDbContext.ElectionAccessCodes.Where(u => u.AccessCode == checkCodeRequest.AccessCode).FirstOrDefaultAsync();
            if (accessCode == null)
            {
                _log.LogDebug("HTTP trigger - CheckAccessCode:End");
                return NotFound(new SecureString { Value = $"AccessCode: '{checkCodeRequest.AccessCode}' not found" });
            }

            // Check if the election (still) exists for this EAC
            var election = await _trueVoteDbContext.Elections.Where(r => r.ElectionId == accessCode.ElectionId).AsNoTracking().OrderByDescending(r => r.DateCreated).FirstOrDefaultAsync();
            if (election == null)
            {
                _log.LogDebug("HTTP trigger - CheckAccessCode:End");
                return NotFound(new SecureString { Value = $"Election: '{accessCode.ElectionId}' not found" });
            }

            // TODO See if the access code was used on a ballot already

            _log.LogDebug("HTTP trigger - CheckAccessCode:End");

            return Ok(election);
        }
    }
}

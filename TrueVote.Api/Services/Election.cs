using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Net.Http.Formatting;
using System.Net;
using TrueVote.Api.Helpers;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;
namespace TrueVote.Api.Services
{
    [ApiController]
    [Produces("application/json")]
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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        public async Task<HttpResponseMessage> CreateElection([FromBody] BaseElectionModel baseElection)
        {
            _log.LogDebug("HTTP trigger - CreateElection:Begin");

            _log.LogInformation($"Request Data: {baseElection}");

            var election = new ElectionModel { Name = baseElection.Name, Description = baseElection.Description, HeaderImageUrl = baseElection.HeaderImageUrl, StartDate = baseElection.StartDate, EndDate = baseElection.EndDate, Races = baseElection.Races };

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Elections.AddAsync(election);
            await _trueVoteDbContext.SaveChangesAsync();

            await _serviceBus.SendAsync($"New TrueVote Election created: {baseElection.Name}");

            _log.LogDebug("HTTP trigger - CreateElection:End");

            return new HttpResponseMessage { StatusCode = HttpStatusCode.Created, Content = new ObjectContent<ElectionModel>(election, new JsonMediaTypeFormatter()) };
        }

        [HttpGet]
        [Route("election/find")]
        [Produces(typeof(ElectionModelList))]
        [Description("Returns collection of Elections")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<HttpResponseMessage> ElectionFind([FromBody] FindElectionModel findElection)
        {
            _log.LogDebug("HTTP trigger - ElectionFind:Begin");

            _log.LogInformation($"Request Data: {findElection}");

            var items = new ElectionModelList
            {
                Elections = await _trueVoteDbContext.Elections
                .Where(e =>
                    findElection.Name == null || (e.Name ?? string.Empty).ToLower().Contains(findElection.Name.ToLower()))
                .OrderByDescending(e => e.DateCreated).ToListAsync()
            };

            _log.LogDebug("HTTP trigger - ElectionFind:End");

            return items.Elections.Count == 0 ? new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound } : new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new ObjectContent<ElectionModelList>(items, new JsonMediaTypeFormatter()) };
        }

        [HttpPost]
        [Route("election/addraces")]
        [Produces(typeof(ElectionModel))]
        [Description("Adds Races to an Election and returns the updated Election")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        public async Task<HttpResponseMessage> AddRaces([FromBody] AddRacesModel bindRaceElectionModel)
        {
            _log.LogDebug("HTTP trigger - AddRaces:Begin");

            _log.LogInformation($"Request Data: {bindRaceElectionModel}");

            // Check if the election exists. If so, return it detached from EF
            var election = await _trueVoteDbContext.Elections.Where(r => r.ElectionId == bindRaceElectionModel.ElectionId).AsNoTracking().OrderByDescending(r => r.DateCreated).FirstOrDefaultAsync();
            if (election == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound, Content = new ObjectContent<SecureString>(new SecureString { Value = $"Election: '{bindRaceElectionModel.ElectionId}' not found" }, new JsonMediaTypeFormatter()) };
            }

            // Check if each Race exists or is already part of the election. If any problems, exit with error
            foreach (var rid in bindRaceElectionModel.RaceIds)
            {
                // Ensure Race exists in Race store
                var race = await _trueVoteDbContext.Races.Where(r => r.RaceId == rid).OrderByDescending(c => c.DateCreated).FirstOrDefaultAsync();
                if (race == null)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound, Content = new ObjectContent<SecureString>(new SecureString { Value = $"Race: '{rid}' not found" }, new JsonMediaTypeFormatter()) };
                }

                // Check if it's already part of the Election
                var raceExists = election.Races?.Where(r => r.RaceId == rid).FirstOrDefault();
                if (raceExists != null)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new ObjectContent<SecureString>(new SecureString { Value = $"Race: '{rid}' already exists in Election" }, new JsonMediaTypeFormatter()) };
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

            return new HttpResponseMessage { StatusCode = HttpStatusCode.Created, Content = new ObjectContent<ElectionModel>(election, new JsonMediaTypeFormatter()) };
        }
    }
}

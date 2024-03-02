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
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status409Conflict)]
    public class Race : ControllerBase
    {
        private readonly ILogger _log;
        private readonly ITrueVoteDbContext _trueVoteDbContext;
        private readonly IServiceBus _serviceBus;

        public Race(ILogger log, ITrueVoteDbContext trueVoteDbContext, IServiceBus serviceBus)
        {
            _log = log;
            _trueVoteDbContext = trueVoteDbContext;
            _serviceBus = serviceBus;
        }

        [HttpPost]
        [Route("race")]
        [Produces(typeof(RaceModel))]
        [Description("Returns the added Race")]
        [ProducesResponseType(typeof(RaceModel), StatusCodes.Status201Created)]
        public async Task<HttpResponseMessage> CreateRace([FromBody] BaseRaceModel baseRace)
        {
            _log.LogDebug("HTTP trigger - CreateRace:Begin");

            _log.LogInformation($"Request Data: {baseRace}");

            var race = new RaceModel { Name = baseRace.Name, RaceType = baseRace.RaceType };

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Races.AddAsync(race);
            await _trueVoteDbContext.SaveChangesAsync();

            await _serviceBus.SendAsync($"New TrueVote Race created: {baseRace.Name}");

            _log.LogDebug("HTTP trigger - CreateRace:End");

            return new HttpResponseMessage { StatusCode = HttpStatusCode.Created, Content = new ObjectContent<RaceModel>(race, new JsonMediaTypeFormatter()) };
        }

        [HttpPost]
        [Route("race/addcandidates")]
        [Produces(typeof(RaceModel))]
        [Description("Adds Candidates to a Race and returns the updated Race")]
        [ProducesResponseType(typeof(RaceModel), StatusCodes.Status201Created)]
        public async Task<HttpResponseMessage> AddCandidates([FromBody] AddCandidatesModel addCandidatesModel)
        {
            _log.LogDebug("HTTP trigger - AddCandidates:Begin");

            _log.LogInformation($"Request Data: {addCandidatesModel}");

            // Check if the race exists. If so, return it detached from EF
            var race = await _trueVoteDbContext.Races.Where(r => r.RaceId == addCandidatesModel.RaceId).AsNoTracking().OrderByDescending(r => r.DateCreated).FirstOrDefaultAsync();
            if (race == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound, Content = new ObjectContent<SecureString>(new SecureString { Value = $"Race: '{addCandidatesModel.RaceId}' not found" }, new JsonMediaTypeFormatter()) };
            }

            // Check if each candidate exists or is already part of the race. If any problems, exit with error
            foreach (var cid in addCandidatesModel.CandidateIds)
            {
                // Ensure Candidate exists in Candidate store
                var candidate = await _trueVoteDbContext.Candidates.Where(c => c.CandidateId == cid).OrderByDescending(c => c.DateCreated).FirstOrDefaultAsync();
                if (candidate == null)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound, Content = new ObjectContent<SecureString>(new SecureString { Value = $"Candidate: '{cid}' not found" }, new JsonMediaTypeFormatter()) };
                }

                // Check if it's already part of the Race
                var candidateExists = race.Candidates?.Where(c => c.CandidateId == cid).FirstOrDefault();
                if (candidateExists != null)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new ObjectContent<SecureString>(new SecureString { Value = $"Candidate: '{cid}' already exists in Race" }, new JsonMediaTypeFormatter()) };
                }

                // Made it this far, add the candidate to the Race. Ok to add here because if another one in the list, it won't get persisted
                race.Candidates.Add(candidate);
            }

            // If made through the loop of checks above, ok to persist. This will write a new Race
            race.DateCreated = UtcNowProviderFactory.GetProvider().UtcNow;
            race.RaceId = Guid.NewGuid().ToString();

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Races.AddAsync(race);
            await _trueVoteDbContext.SaveChangesAsync();

            _log.LogDebug("HTTP trigger - AddCandidates:End");

            return new HttpResponseMessage { StatusCode = HttpStatusCode.Created, Content = new ObjectContent<RaceModel>(race, new JsonMediaTypeFormatter()) };
        }

        [HttpGet]
        [Route("race/find")]
        [Produces(typeof(RaceModelList))]
        [Description("Returns collection of Races")]
        [ProducesResponseType(typeof(RaceModelList), StatusCodes.Status200OK)]
        public async Task<HttpResponseMessage> RaceFind([FromBody] FindRaceModel findRace)
        {
            _log.LogDebug("HTTP trigger - RaceFind:Begin");

            _log.LogInformation($"Request Data: {findRace}");

            // TODO Fix RaceTypeName not resolving properly
            // Get all the races that match the search
            var items = new RaceModelList
            {
                Races = await _trueVoteDbContext.Races
                .Where(r =>
                    findRace.Name == null || (r.Name ?? string.Empty).ToLower().Contains(findRace.Name.ToLower()))
                .OrderByDescending(r => r.DateCreated).ToListAsync()
            };

            _log.LogDebug("HTTP trigger - RaceFind:End");

            return items.Races.Count == 0 ? new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound } : new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new ObjectContent<RaceModelList>(items, new JsonMediaTypeFormatter()) };
        }
    }
}

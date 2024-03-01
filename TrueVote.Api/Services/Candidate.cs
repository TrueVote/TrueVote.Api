using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Net.Http.Formatting;
using System.Net;
using TrueVote.Api2.Interfaces;
using TrueVote.Api2.Models;

namespace TrueVote.Api2.Services
{
    [ApiController]
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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        public async Task<HttpResponseMessage> CreateCandidate([FromBody] BaseCandidateModel baseCandidate)
        {
            _log.LogDebug("HTTP trigger - CreateCandidate:Begin");

            _log.LogInformation($"Request Data: {baseCandidate}");

            var candidate = new CandidateModel { Name = baseCandidate.Name, PartyAffiliation = baseCandidate.PartyAffiliation };

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Candidates.AddAsync(candidate);
            await _trueVoteDbContext.SaveChangesAsync();

            await _serviceBus.SendAsync($"New TrueVote Candidate created: {baseCandidate.Name}");

            _log.LogDebug("HTTP trigger - CreateCandidate:End");

            return new HttpResponseMessage { StatusCode = HttpStatusCode.Created, Content = new ObjectContent<CandidateModel>(candidate, new JsonMediaTypeFormatter()) };
        }

        [HttpGet]
        [Route("candidate/find")]
        [Produces(typeof(CandidateModelList))]
        [Description("Returns collection of Candidates")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<HttpResponseMessage> CandidateFind([FromBody] FindCandidateModel findCandidate)
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

            return items.Candidates.Count == 0 ? new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound } : new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new ObjectContent<CandidateModelList>(items, new JsonMediaTypeFormatter()) };
        }
    }
}

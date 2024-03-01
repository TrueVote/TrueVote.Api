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
    public class Timestamp : ControllerBase
    {
        private readonly ILogger _log;
        private readonly ITrueVoteDbContext _trueVoteDbContext;

        public Timestamp(ILogger log, ITrueVoteDbContext trueVoteDbContext)
        {
            _log = log;
            _trueVoteDbContext = trueVoteDbContext;
        }

        [HttpGet]
        [Route("timestamp/find")]
        [Produces(typeof(List<TimestampModel>))]
        [Description("Returns List of Timestamps by Date")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<HttpResponseMessage> TimestampFind([FromBody] FindTimestampModel findTimestamp)
        {
            _log.LogDebug("HTTP trigger - TimestampFind:Begin");

            _log.LogInformation($"Request Data: {findTimestamp}");

            var items = await _trueVoteDbContext.Timestamps
                .Where(c => c.DateCreated >= findTimestamp.DateCreatedStart && c.DateCreated <= findTimestamp.DateCreatedEnd)
                .OrderByDescending(c => c.DateCreated).ToListAsync();

            _log.LogDebug("HTTP trigger - TimestampFind:End");

            return items.Count == 0 ? new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound } : new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new ObjectContent<List<TimestampModel>>(items, new JsonMediaTypeFormatter()) };
        }
    }
}

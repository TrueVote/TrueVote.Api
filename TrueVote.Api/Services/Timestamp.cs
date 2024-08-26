using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
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
        [ProducesResponseType(typeof(List<TimestampModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> TimestampFind([FromQuery] FindTimestampModel findTimestamp)
        {
            _log.LogDebug("HTTP trigger - TimestampFind:Begin");

            _log.LogInformation($"Request Data: {findTimestamp}");

            var items = await _trueVoteDbContext.Timestamps
                .Where(c => c.DateCreated >= findTimestamp.DateCreatedStart && c.DateCreated <= findTimestamp.DateCreatedEnd)
                .OrderByDescending(c => c.DateCreated).ToListAsync();

            _log.LogDebug("HTTP trigger - TimestampFind:End");

            return items.Count == 0 ? NotFound() : Ok(items);
        }
    }
}

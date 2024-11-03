using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status422UnprocessableEntity)]
    public class Comms : ControllerBase
    {
        private readonly ILogger _log;
        private readonly ITrueVoteDbContext _trueVoteDbContext;

        public Comms(ILogger log, ITrueVoteDbContext trueVoteDbContext)
        {
            _log = log;
            _trueVoteDbContext = trueVoteDbContext;
        }

        [HttpPut]
        [Authorize]
        [RequireRole(UserRoles.Service_Role)]
        [Route("comms/events/updatecommeventstatus")]
        [Produces(typeof(CommunicationEventModel))]
        [Description("Updates a Comms Event Status")]
        [ProducesResponseType(typeof(CommunicationEventModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCommEventStatus([FromBody] CommunicationEventUpdateModel communicationEventUpdateModel)
        {
            _log.LogDebug("HTTP trigger - UpdateCommEventStatus:Begin");
            _log.LogInformation($"Request Data: {communicationEventUpdateModel}");

            try
            {
                var commEvent = await _trueVoteDbContext.CommunicationEvents.Where(c => c.CommunicationEventId == communicationEventUpdateModel.CommunicationEventId).FirstOrDefaultAsync();
                if (commEvent == null)
                {
                    _log.LogDebug("HTTP trigger - UpdateCommEventStatus:End");

                    return NotFound(new SecureString { Value = $"Communication Event '{communicationEventUpdateModel.CommunicationEventId}' not found" });
                }

                // Update fields
                commEvent.Status = communicationEventUpdateModel.Status;
                commEvent.DateUpdated = UtcNowProviderFactory.GetProvider().UtcNow;
                commEvent.DateProcessed = communicationEventUpdateModel.DateProcessed;
                commEvent.ErrorMessage = communicationEventUpdateModel.ErrorMessage;

                _trueVoteDbContext.CommunicationEvents.Update(commEvent);
                await _trueVoteDbContext.SaveChangesAsync();

                _log.LogDebug("HTTP trigger - UpdateCommEventStatus:End");

                return Ok(commEvent);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error updating communication event status");

                return UnprocessableEntity(new SecureString { Value = ex.Message });
            }
        }
    }
}

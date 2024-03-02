using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;
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
    public class Error500 : ControllerBase
    {
        private readonly ILogger _log;
        private readonly IServiceBus _serviceBus;

        public Error500(ILogger log, IServiceBus serviceBus)
        {
            _log = log;
            _serviceBus = serviceBus;
        }

        [HttpGet]
        [Route("error500")]
        [Description("Tests Error Logging of a Server 500")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SecureString), StatusCodes.Status500InternalServerError)]
        public async Task<HttpResponseMessage> ThrowError500([FromBody] dynamic data)
        {
            _log.LogDebug("HTTP trigger - ThrowError500:Begin");

            _log.LogInformation($"Request Data: {data}");

            if (data?.Error == "true")
            {
                // Throw this random exception for no reason other than the requester wants it
                _log.LogError($"error500 - throwing a sample exception");
                _log.LogDebug("HTTP trigger - ThrowError500:End");
                await _serviceBus.SendAsync($"error500 - throwing a sample exception");
                throw new Exception("error500 - throwing a sample exception");
            }

            _log.LogDebug("HTTP trigger - ThrowError500:End");

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}

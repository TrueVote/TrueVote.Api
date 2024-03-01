using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;

namespace TrueVote.Api.Services
{
    [ApiController]
    [Produces("application/json")]
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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
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

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;
using TrueVote.Api.Models;

namespace TrueVote.Api.Services
{
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class Status : ControllerBase
    {
        private static BuildInfo _BuildInfo = null;
        private static string _BuildInfoReadTime = null;
        private readonly ILogger _log;
        private readonly IServiceBus _serviceBus;

        public Status(ILogger log, IServiceBus serviceBus)
        {
            _log = log;
            _serviceBus = serviceBus;
        }

        [HttpGet]
        [Route("status")]
        [Produces(typeof(StatusModel))]
        [Description("Returns Status of Api")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> GetStatus()
        {
            _log.LogDebug("HTTP trigger - GetStatus:Begin");

            // For timing the running of this function
            var watch = Stopwatch.StartNew();

            var status = new StatusModel();

            // Basic message just to show it's responding
            status.Responds = true;
            status.RespondsMsg = "TrueVote.Api is responding";

            // Check if the static is already been initialized. If not, fetch the properties from the Version.cs static.
            if (_BuildInfo == null)
            {
                // Marshall the contents of the Version.cs static into a model
                _BuildInfo = JsonConvert.DeserializeObject<BuildInfo>(VersionInfo.BuildInfo);

                // Convert the time for consistency
                if (_BuildInfo.BuildTime != string.Empty)
                {
                    _BuildInfo.BuildTime = $"{DateTime.Parse(_BuildInfo.BuildTime)} UTC";
                }

                // Set the read time to now. This should never change because it's stored in a static.
                _BuildInfoReadTime = DateTime.Now.ToUniversalTime().ToString("dddd, MMM dd, yyyy HH:mm:ss");
            }

            // Attach the static to the returned object
            status.BuildInfo = _BuildInfo;
            status.BuildInfoReadTime = _BuildInfoReadTime;

            // Stop running
            watch.Stop();

            status.ExecutionTime = watch.ElapsedMilliseconds;
            status.ExecutionTimeMsg = $"Time to run: {watch.ElapsedMilliseconds}ms";
            status.CurrentTime = DateTime.Now.ToUniversalTime().ToString("dddd, MMM dd, yyyy HH:mm:ss");

            await _serviceBus.SendAsync($"Status Check");

            _log.LogDebug("HTTP trigger - GetStatus:End");

            return Ok(status);
        }
    }
}

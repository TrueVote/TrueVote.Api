using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using TrueVote.Api.Models;

namespace TrueVote.Api
{
    public class Status
    {
        protected IFileSystem _fileSystem;
        protected ILogger _log;
        public static BuildInfo _BuildInfo = null;
        protected static string _BuildInfoReadTime = null;
        protected static string _BinDir = Assembly.GetExecutingAssembly().CodeBase
                .Replace(Assembly.GetExecutingAssembly().CodeBase.Split('/').Last(), "")
                .Replace("file:///", "");

        public Status(IFileSystem fileSystem, ILogger log, bool clearStatics = false)
        {
            _fileSystem = fileSystem;
            _log = log;
            if (clearStatics)
            {
                ClearStatics();
            }
        }

        private void ClearStatics()
        {
            _BuildInfo = null;
            _BuildInfoReadTime = null;
        }

        [FunctionName("status")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "Status" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(StatusModel), Description = "Returns Status of Api")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _log.LogDebug("HTTP trigger - Status:Begin");

            // For timing the running of this function
            var watch = Stopwatch.StartNew();

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            _log.LogInformation($"Request Data: {data}");

            var status = new StatusModel();

            // Basic message just to show it's responding
            status.Responds = true;
            status.RespondsMsg = "TrueVote.Api is responding";

            // Check if the static is already been initialized. If not, fetch the properties from the version.json file and set them.
            if (_BuildInfo == null)
            {
                // Read the version file
                var buildInfoString = GetBuildInfo();
                if (buildInfoString != null)
                {
                    // Marshall the contents of the version.json file into a model
                    _BuildInfo = JsonConvert.DeserializeObject<BuildInfo>(buildInfoString);

                    // Convert the time for consistency
                    _BuildInfo.BuildTime ??= $"{DateTime.Parse(_BuildInfo.BuildTime)} UTC";

                    // Set the read time to now. This should never change because it's stored in a static.
                    _BuildInfoReadTime = DateTime.Now.ToUniversalTime().ToString("dddd, MMM dd, yyyy HH:mm:ss");
                }
            }

            // Attach the static to the returned object
            status.BuildInfo = _BuildInfo;
            status.BuildInfoReadTime = _BuildInfoReadTime;

            // Stop running
            watch.Stop();

            status.ExecutionTime = watch.ElapsedMilliseconds;
            status.ExecutionTimeMsg = $"Time to run: {watch.ElapsedMilliseconds}ms";
            status.CurrentTime = DateTime.Now.ToUniversalTime().ToString("dddd, MMM dd, yyyy HH:mm:ss");

            _log.LogDebug("HTTP trigger - Status:End");

            return new OkObjectResult(status);
        }

        private string GetBuildInfo()
        {
            _log.LogInformation($"binDir: {_BinDir}");

            var binDir = _BinDir;

            // On Linux we may need to add a leading /
            if (!_fileSystem.Path.IsPathFullyQualified(binDir))
            {
                binDir = "/" + binDir;
                _log.LogInformation("Added leading '/' to binDir");
                _log.LogInformation($"Modified binDir: {binDir}");
            }

            try
            {
                var versionFile = _fileSystem.Path.Combine(binDir, "version.json");

                _log.LogInformation($"Loading build info from: {versionFile}");

                var versionContents = _fileSystem.File.ReadAllText(versionFile);

                _log.LogInformation($"Loaded build info from version.json");

                return versionContents;
            }
            catch (Exception e)
            {
                _log.LogError($"Could not load version.json file. Exception: {e.Message}");

                return null;
            }
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
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
using TrueVote.Api.Helpers;
using TrueVote.Api.Models;

namespace TrueVote.Api
{
    public class Status : LoggerHelper
    {
        protected IFileSystem _fileSystem;
        public static BuildInfo _BuildInfo = null;
        private static string _BuildInfoReadTime = null;

        public Status(IFileSystem fileSystem, ILogger log, bool clearStatics = false): base(log)
        {
            _fileSystem = fileSystem;
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

        [FunctionName(nameof(GetStatus))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [OpenApiOperation(operationId: "GetStatus", tags: new[] { "Status" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(StatusModel), Description = "Returns Status of Api")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        public async Task<IActionResult> GetStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequest req)
        {
            _log.LogDebug("HTTP trigger - GetStatus:Begin");

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
                    if (_BuildInfo.BuildTime != string.Empty)
                    {
                        _BuildInfo.BuildTime = $"{DateTime.Parse(_BuildInfo.BuildTime)} UTC";
                    }

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

            _log.LogDebug("HTTP trigger - GetStatus:End");

            return new OkObjectResult(status);
        }

        private string GetBuildInfo()
        {
            var codeBase = new Uri(Assembly.GetExecutingAssembly().Location).ToString();
            var binDir = codeBase.Replace(codeBase.Split('/').Last(), "");
            binDir = binDir.Remove(binDir.LastIndexOf("bin/"));
            if (binDir.Contains(".Tests/")) {
                binDir = binDir.Remove(binDir.LastIndexOf(".Tests/"));
            }
            binDir = binDir.Replace("file:///", "");

            _log.LogInformation($"binDir: {binDir}");

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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Net;
using System.Threading.Tasks;
using TrueVote.Api.Helpers;
using TrueVote.Api.Models;

namespace TrueVote.Api.Services
{
    public class Status : LoggerHelper
    {
        private static BuildInfo _BuildInfo = null;
        private static string _BuildInfoReadTime = null;

        public Status(ILogger log): base(log)
        {
        }

        [Function(nameof(GetStatus))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [OpenApiOperation(operationId: "GetStatus", tags: new[] { "Status" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(StatusModel), Description = "Returns Status of Api")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        public async Task<HttpResponseData> GetStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequestData req)
        {
            LogDebug("HTTP trigger - GetStatus:Begin");

            // For timing the running of this function
            var watch = Stopwatch.StartNew();

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            LogInformation($"Request Data: {data}");

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

            LogDebug("HTTP trigger - GetStatus:End");

            return await req.CreateOkResponseAsync(status);
        }
    }
}

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
using System.IO;
using System.Net;
using System.Threading.Tasks;
using TrueVote.Api.Helpers;
using TrueVote.Api.Models;

namespace TrueVote.Api
{
    public class Error500 : LoggerHelper
    {
        public Error500(ILogger log): base(log)
        {
        }

        [FunctionName(nameof(ThrowError500))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [OpenApiOperation(operationId: "ThrowError500", tags: new[] { "Errors" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SecureString), Description = "Tests Error Logging of a Server 500")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        public async Task<IActionResult> ThrowError500(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "error500")] HttpRequest req)
        {
            _log.LogDebug("HTTP trigger - ThrowError500:Begin");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            _log.LogInformation($"Request Data: {data}");

            if (data?.Error == "true")
            {
                // Throw this random exception for no reason other than the requester wants it
                _log.LogError($"error500 - throwing a sample exception");
                _log.LogDebug("HTTP trigger - ThrowError500:End");
                throw new Exception("error500 - throwing a sample exception");
            }

            _log.LogDebug("HTTP trigger - ThrowError500:End");

            return new OkResult();
        }
    }
}

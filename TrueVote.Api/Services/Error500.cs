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

namespace TrueVote.Api
{
    public class Error500 : LoggerHelper
    {
        public Error500(ILogger log): base(log)
        {
        }

        [FunctionName("error500")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [OpenApiOperation(operationId: "Run", tags: new[] { "Error500" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Tests Error Logging of a Server 500")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _log.LogDebug("HTTP trigger - Error500:Begin");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            _log.LogInformation($"Request Data: {data}");

            if (data?.Error == "true")
            {
                // Throw this random exception for no reason other than the requester wants it
                throw new Exception("error500 - throwing an exception");
            }

            _log.LogDebug("HTTP trigger - Error500:End");

            return new OkResult();
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using TrueVote.Api.Helpers;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;

namespace TrueVote.Api.Services
{
    public class Timestamp : LoggerHelper
    {
        private readonly ITrueVoteDbContext _trueVoteDbContext;
        private readonly TelegramBot _telegramBot;

        public Timestamp(ILogger log, ITrueVoteDbContext trueVoteDbContext, TelegramBot telegramBot) : base(log, telegramBot)
        {
            _trueVoteDbContext = trueVoteDbContext;
            _telegramBot = telegramBot;
        }

        [Function(nameof(TimestampFind))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [OpenApiOperation(operationId: "TimestampFind", tags: new[] { "Timestamp" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(FindTimestampModel), Description = "Fields to search for Timestamps", Example = typeof(FindTimestampModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CandidateModelList), Description = "Returns collection of Candidates")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        public async Task<IActionResult> TimestampFind(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "timestamp/find")] HttpRequestData req)
        {
            LogDebug("HTTP trigger - TimestampFind:Begin");

            FindTimestampModel findTimestamp;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                findTimestamp = JsonConvert.DeserializeObject<FindTimestampModel>(requestBody);
            }
            catch (Exception e)
            {
                LogError("findTimestamp: invalid format");
                LogDebug("HTTP trigger - TimestampFind:End");

                return new BadRequestObjectResult(e.Message);
            }

            LogInformation($"Request Data: {findTimestamp}");

            var items = await _trueVoteDbContext.Timestamps
                .Where(c => c.DateCreated >= findTimestamp.DateCreatedStart && c.DateCreated <= findTimestamp.DateCreatedEnd)
                .OrderByDescending(c => c.DateCreated).ToListAsync();

            LogDebug("HTTP trigger - TimestampFind:End");

            return items.Count == 0 ? new NotFoundResult() : new OkObjectResult(items);
        }
    }
}

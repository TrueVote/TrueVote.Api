using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
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
    public class Ballot : LoggerHelper
    {
        private readonly ITrueVoteDbContext _trueVoteDbContext;
        private readonly TelegramBot _telegramBot;

        public Ballot(ILogger log, ITrueVoteDbContext trueVoteDbContext, TelegramBot telegramBot) : base(log, telegramBot)
        {
            _trueVoteDbContext = trueVoteDbContext;
            _telegramBot = telegramBot;
        }

        [FunctionName(nameof(SubmitBallot))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [OpenApiOperation(operationId: "SubmitBallot", tags: new[] { "Ballot" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(SubmitBallotModel), Description = "Election Model with vote selections", Example = typeof(SubmitBallotModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(SubmitBallotModelResponse), Description = "Returns the Ballot submission status")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnsupportedMediaType, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unsupported Media Type")]
        public async Task<IActionResult> SubmitBallot(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ballot/submitballot")] HttpRequest req) {
            LogDebug("HTTP trigger - SubmitBallot:Begin");

            SubmitBallotModel bindSubmitBallotModel;
            try {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                bindSubmitBallotModel = JsonConvert.DeserializeObject<SubmitBallotModel>(requestBody);
            }
            catch (Exception e) {
                LogError("bindSubmitBallotModel: invalid format");
                LogDebug("HTTP trigger - SubmitBallot:End");

                return new BadRequestObjectResult(e.Message);
            }

            LogInformation($"Request Data: {bindSubmitBallotModel}");

            // TODO Validate the ballot and return a response

            var submitBallotResponse = new SubmitBallotModelResponse {
                ElectionId = bindSubmitBallotModel.ElectionId,
                Message = $"Ballot successfully submitted. Election ID: {bindSubmitBallotModel.ElectionId}"
            };

            await _telegramBot.SendChannelMessageAsync($"New TrueVote Ballot successfully submitted. Election ID: {bindSubmitBallotModel.ElectionId}");

            LogDebug("HTTP trigger - SubmitBallot:End");

            // TODO Return a Ballot Submitted model response with more key data
            return new CreatedResult(string.Empty, submitBallotResponse);
        }
    }
}

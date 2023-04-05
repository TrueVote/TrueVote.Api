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

            // TODO Validate the ballot

            var ballot = new BallotModel { ElectionId = bindSubmitBallotModel.ElectionId, Election = bindSubmitBallotModel.Election };

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Ballots.AddAsync(ballot);
            await _trueVoteDbContext.SaveChangesAsync();

            var submitBallotResponse = new SubmitBallotModelResponse {
                ElectionId = bindSubmitBallotModel.ElectionId,
                BallotId = ballot.BallotId,
                Message = $"Ballot successfully submitted. Election ID: {bindSubmitBallotModel.ElectionId}, Ballot ID: {ballot.BallotId}"
            };

            await _telegramBot.SendChannelMessageAsync($"New TrueVote Ballot successfully submitted. Election ID: {bindSubmitBallotModel.ElectionId}, Ballot ID: {ballot.BallotId}");

            LogDebug("HTTP trigger - SubmitBallot:End");

            // TODO Return a Ballot Submitted model response with critical key data to bind ballot / user
            return new CreatedResult(string.Empty, submitBallotResponse);
        }

        [FunctionName(nameof(BallotFind))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [OpenApiOperation(operationId: "BallotFind", tags: new[] { "Ballot" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(FindBallotModel), Description = "Fields to search for Ballots", Example = typeof(FindBallotModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(BallotModelList), Description = "Returns collection of Ballots")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        public async Task<IActionResult> BallotFind(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ballot/find")] HttpRequest req)
        {
            LogDebug("HTTP trigger - BallotFind:Begin");

            FindBallotModel findBallot;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                findBallot = JsonConvert.DeserializeObject<FindBallotModel>(requestBody);
            }
            catch (Exception e)
            {
                LogError("findBallot: invalid format");
                LogDebug("HTTP trigger - BallotFind:End");

                return new BadRequestObjectResult(e.Message);
            }

            LogInformation($"Request Data: {findBallot}");

            var items = await _trueVoteDbContext.Ballots
                .Where(e =>
                    findBallot.BallotId == null || (e.BallotId ?? string.Empty).ToLower().Contains(findBallot.BallotId.ToLower()))
                .OrderByDescending(e => e.DateCreated).ToListAsync();

            LogDebug("HTTP trigger - BallotFind:End");

            return items.Count == 0 ? new NotFoundResult() : new OkObjectResult(items);
        }
    }
}

using System;
using System.Collections.Generic;
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
        private readonly Validator _validator;

        public Ballot(ILogger log, ITrueVoteDbContext trueVoteDbContext, TelegramBot telegramBot, Validator validator) : base(log, telegramBot)
        {
            _trueVoteDbContext = trueVoteDbContext;
            _telegramBot = telegramBot;
            _validator = validator;
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

            var ballot = new BallotModel { ElectionId = bindSubmitBallotModel.ElectionId, Election = bindSubmitBallotModel.Election, ClientBallotHash = bindSubmitBallotModel.ClientBallotHash };

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Ballots.AddAsync(ballot);
            await _trueVoteDbContext.SaveChangesAsync();

            var submitBallotResponse = new SubmitBallotModelResponse {
                ElectionId = bindSubmitBallotModel.ElectionId,
                BallotId = ballot.BallotId,
                Message = $"Ballot successfully submitted. Election ID: {bindSubmitBallotModel.ElectionId}, Ballot ID: {ballot.BallotId}"
            };

            await _telegramBot.SendChannelMessageAsync($"New TrueVote Ballot successfully submitted. Election ID: {bindSubmitBallotModel.ElectionId}, Ballot ID: {ballot.BallotId}");

            // TODO Post a message to Service Bus for this Ballot
            // FOR NOW ONLY - THIS LINE SHOULD BE REPLACED WITH A POST TO SERVICE BUS
            // Hash the ballot
            await _validator.HashBallotAsync(ballot);

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

        [FunctionName(nameof(BallotCount))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [OpenApiOperation(operationId: "BallotCount", tags: new[] { "Ballot" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CountBallotModel), Description = "Fields to search for Ballots", Example = typeof(CountBallotModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(int), Description = "Returns count of Ballots")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        public async Task<IActionResult> BallotCount(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ballot/count")] HttpRequest req)
        {
            LogDebug("HTTP trigger - BallotCount:Begin");

            CountBallotModel countBallot;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                countBallot = JsonConvert.DeserializeObject<CountBallotModel>(requestBody);
            }
            catch (Exception e)
            {
                LogError("ballotCount: invalid format");
                LogDebug("HTTP trigger - BallotCount:End");

                return new BadRequestObjectResult(e.Message);
            }

            LogInformation($"Request Data: {countBallot}");

            var items = await _trueVoteDbContext.Ballots
                .Where(c => c.DateCreated >= countBallot.DateCreatedStart && c.DateCreated <= countBallot.DateCreatedEnd)
                .OrderByDescending(c => c.DateCreated).ToListAsync();

            LogDebug("HTTP trigger - BallotFind:End");

            return new OkObjectResult(items.Count);
        }

        [FunctionName(nameof(BallotHashFind))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [OpenApiOperation(operationId: "BallotHashFind", tags: new[] { "Ballot" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(FindBallotHashModel), Description = "Fields to search for Ballot Hashes", Example = typeof(FindBallotHashModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<BallotHashModel>), Description = "Returns collection of Ballot Hashes")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        public async Task<IActionResult> BallotHashFind(
                    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ballot/findhash")] HttpRequest req)
        {
            LogDebug("HTTP trigger - BallotHashFind:Begin");

            FindBallotHashModel findBallotHash;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                findBallotHash = JsonConvert.DeserializeObject<FindBallotHashModel>(requestBody);
            }
            catch (Exception e)
            {
                LogError("findBallotHash: invalid format");
                LogDebug("HTTP trigger - BallotHashFind:End");

                return new BadRequestObjectResult(e.Message);
            }

            LogInformation($"Request Data: {findBallotHash}");

            var items = await _trueVoteDbContext.BallotHashes
                .Where(e =>
                    findBallotHash.BallotId == null || (e.BallotId ?? string.Empty).ToLower().Contains(findBallotHash.BallotId.ToLower()))
                .OrderByDescending(e => e.DateCreated).ToListAsync();

            LogDebug("HTTP trigger - BallotHashFind:End");

            return items.Count == 0 ? new NotFoundResult() : new OkObjectResult(items);
        }
    }
}

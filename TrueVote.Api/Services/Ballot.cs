using System;
using System.Collections.Generic;
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
    public class Ballot : LoggerHelper
    {
        private readonly ITrueVoteDbContext _trueVoteDbContext;
        private readonly IValidator _validator;

        public Ballot(ILogger log, ITrueVoteDbContext trueVoteDbContext, IValidator validator) : base(log)
        {
            _trueVoteDbContext = trueVoteDbContext;
            _validator = validator;
        }

        [Function(nameof(SubmitBallot))]
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
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Conflict, contentType: "application/json", bodyType: typeof(SecureString), Description = "Conflict with input model")]
        public async Task<HttpResponseData> SubmitBallot(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ballot/submitballot")] HttpRequestData req) {
            LogDebug("HTTP trigger - SubmitBallot:Begin");

            SubmitBallotModel bindSubmitBallotModel;
            try {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                bindSubmitBallotModel = JsonConvert.DeserializeObject<SubmitBallotModel>(requestBody);
            }
            catch (Exception e) {
                LogError("bindSubmitBallotModel: invalid format");
                LogDebug("HTTP trigger - SubmitBallot:End");

                return await req.CreateBadRequestResponseAsync(new SecureString { Value = e.Message });
            }

            LogInformation($"Request Data: {bindSubmitBallotModel}");

            // TODO Validate the ballot
            // 1. Must have a UserId and not have already submitted a ballot for this election
            // 2. Confirm the election id exists
            // 3. Confirm the election data for this ballot has not been altered.
            // ADD CODE FOR ABOVE ITEMS HERE

            var ballot = new BallotModel { Election = bindSubmitBallotModel.Election };
            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Ballots.AddAsync(ballot);
            await _trueVoteDbContext.SaveChangesAsync();

            // TODO Localize .Message
            var submitBallotResponse = new SubmitBallotModelResponse {
                ElectionId = bindSubmitBallotModel.Election.ElectionId,
                BallotId = ballot.BallotId,
                Message = $"Ballot successfully submitted. Election ID: {bindSubmitBallotModel.Election.ElectionId}, Ballot ID: {ballot.BallotId}"
            };

            // TODO Post a message to Service Bus for this Ballot
            // FOR NOW ONLY - THIS LINE SHOULD BE REPLACED WITH A POST TO SERVICE BUS
            // Hash the ballot
            try
            {
                await _validator.HashBallotAsync(ballot);
            }
            catch (Exception e)
            {
                LogError("HashBallotAsync()");
                LogDebug("HTTP trigger - SubmitBallot:End");

                var msg = submitBallotResponse.Message += " - Failure Hashing: " + e.Message;

                return await req.CreateConflictResponseAsync(new SecureString { Value = msg });
            }

            LogDebug("HTTP trigger - SubmitBallot:End");

            // TODO Return a Ballot Submitted model response with critical key data to bind ballot / user
            return await req.CreateCreatedResponseAsync(submitBallotResponse);
        }

        [Function(nameof(BallotFind))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [OpenApiOperation(operationId: "BallotFind", tags: new[] { "Ballot" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(FindBallotModel), Description = "Fields to search for Ballots", Example = typeof(FindBallotModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(BallotList), Description = "Returns collection of Ballots")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        public async Task<HttpResponseData> BallotFind(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ballot/find")] HttpRequestData req)
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

                return await req.CreateBadRequestResponseAsync(new SecureString { Value = e.Message });
            }

            LogInformation($"Request Data: {findBallot}");

            var items = new BallotList
            {
                Ballots = await _trueVoteDbContext.Ballots
                .Where(e =>
                    findBallot.BallotId == null || (e.BallotId ?? string.Empty).ToLower().Contains(findBallot.BallotId.ToLower()))
                .OrderByDescending(e => e.DateCreated).ToListAsync(),
                BallotHashes = await _trueVoteDbContext.BallotHashes
                .Where(e =>
                    findBallot.BallotId == null || (e.BallotId ?? string.Empty).ToLower().Contains(findBallot.BallotId.ToLower()))
                .OrderByDescending(e => e.DateCreated).ToListAsync()
            };

            LogDebug("HTTP trigger - BallotFind:End");

            return items.Ballots.Count == 0 ? req.CreateNotFoundResponse() : await req.CreateOkResponseAsync(items);
        }

        [Function(nameof(BallotCount))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [OpenApiOperation(operationId: "BallotCount", tags: new[] { "Ballot" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CountBallotModel), Description = "Fields to search for Ballots", Example = typeof(CountBallotModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CountBallotModelResponse), Description = "Returns count of Ballots")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        public async Task<HttpResponseData> BallotCount(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ballot/count")] HttpRequestData req)
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

                return await req.CreateBadRequestResponseAsync(new SecureString { Value = e.Message });
            }

            LogInformation($"Request Data: {countBallot}");

            var items = await _trueVoteDbContext.Ballots
                .Where(c => c.DateCreated >= countBallot.DateCreatedStart && c.DateCreated <= countBallot.DateCreatedEnd)
                .OrderByDescending(c => c.DateCreated).ToListAsync();

            var ballotCountModel = new CountBallotModelResponse { BallotCount = items.Count };

            LogDebug("HTTP trigger - BallotCount:End");

            return await req.CreateOkResponseAsync(ballotCountModel);
        }

        [Function(nameof(BallotHashFind))]
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
        public async Task<HttpResponseData> BallotHashFind(
                    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ballot/findhash")] HttpRequestData req)
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

                return await req.CreateBadRequestResponseAsync(new SecureString { Value = e.Message });
            }

            LogInformation($"Request Data: {findBallotHash}");

            var items = await _trueVoteDbContext.BallotHashes
                .Where(e =>
                    findBallotHash.BallotId == null || (e.BallotId ?? string.Empty).ToLower().Contains(findBallotHash.BallotId.ToLower()))
                .OrderByDescending(e => e.DateCreated).ToListAsync();

            LogDebug("HTTP trigger - BallotHashFind:End");

            return items.Count == 0 ? req.CreateNotFoundResponse() : await req.CreateOkResponseAsync(items);
        }
    }
}

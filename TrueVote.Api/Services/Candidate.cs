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
using TrueVote.Api.Models;

namespace TrueVote.Api.Services
{
    public class Candidate : LoggerHelper
    {
        private readonly TrueVoteDbContext _trueVoteDbContext;

        public Candidate(ILogger log, TrueVoteDbContext trueVoteDbContext) : base(log)
        {
            _trueVoteDbContext = trueVoteDbContext;
        }

        [FunctionName(nameof(CreateCandidate))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [OpenApiOperation(operationId: "CreateCandidate", tags: new[] { "Candidate" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(BaseCandidateModel), Description = "Partially filled Candidate Model", Example = typeof(BaseCandidateModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(CandidateModel), Description = "Returns the added candidate")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnsupportedMediaType, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unsupported Media Type")]
        public async Task<IActionResult> CreateCandidate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "candidate")] HttpRequest req)
        {
            _log.LogDebug("HTTP trigger - CreateCandidate:Begin");

            BaseCandidateModel baseCandidate;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                baseCandidate = JsonConvert.DeserializeObject<BaseCandidateModel>(requestBody);
            }
            catch (Exception e)
            {
                _log.LogError("baseCandidate: invalid format");
                _log.LogDebug("HTTP trigger - CreateCandidate:End");

                return new BadRequestObjectResult(e.Message);
            }

            _log.LogInformation($"Request Data: {baseCandidate}");

            var candidate = new CandidateModel { Name = baseCandidate.Name, PartyAffiliation = baseCandidate.PartyAffiliation };

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Candidates.AddAsync(candidate);
            await _trueVoteDbContext.SaveChangesAsync();

            _log.LogDebug("HTTP trigger - CreateCandidate:End");

            return new CreatedResult(string.Empty, candidate);
        }

        [FunctionName(nameof(CandidateFind))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [OpenApiOperation(operationId: "CandidateFind", tags: new[] { "Candidate" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(FindCandidateModel), Description = "Fields to search for Candidates", Example = typeof(FindCandidateModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CandidateModelList), Description = "Returns collection of candidates")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        public async Task<IActionResult> CandidateFind(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "candidate/find")] HttpRequest req)
        {
            _log.LogDebug("HTTP trigger - CandidateFind:Begin");

            FindCandidateModel findCandidate;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                findCandidate = JsonConvert.DeserializeObject<FindCandidateModel>(requestBody);
            }
            catch (Exception e)
            {
                _log.LogError("findCandidate: invalid format");
                _log.LogDebug("HTTP trigger - CandidateFind:End");

                return new BadRequestObjectResult(e.Message);
            }

            _log.LogInformation($"Request Data: {findCandidate}");

            var items = await _trueVoteDbContext.Candidates
                .Where(c =>
                    (findCandidate.Name == null || (c.Name ?? string.Empty).Contains(findCandidate.Name, StringComparison.InvariantCultureIgnoreCase)) &&
                    (findCandidate.PartyAffiliation == null || (c.PartyAffiliation ?? string.Empty).Contains(findCandidate.PartyAffiliation, StringComparison.InvariantCultureIgnoreCase)))
                .OrderByDescending(c => c.DateCreated).ToListAsync();

            _log.LogDebug("HTTP trigger - CandidateFind:End");

            return new OkObjectResult(items);
        }
    }
}

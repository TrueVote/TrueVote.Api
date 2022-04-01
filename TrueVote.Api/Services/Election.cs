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
    public class Election : LoggerHelper
    {
        private readonly TrueVoteDbContext _trueVoteDbContext;

        public Election(ILogger log, TrueVoteDbContext trueVoteDbContext) : base(log)
        {
            _trueVoteDbContext = trueVoteDbContext;
        }

        [FunctionName(nameof(CreateElection))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [OpenApiOperation(operationId: "CreateElection", tags: new[] { "Election" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(BaseElectionModel), Description = "Partially filled Election Model", Example = typeof(BaseElectionModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(ElectionModel), Description = "Returns the added Election")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnsupportedMediaType, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unsupported Media Type")]
        public async Task<IActionResult> CreateElection(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "election")] HttpRequest req)
        {
            _log.LogDebug("HTTP trigger - CreateElection:Begin");

            BaseElectionModel baseElection;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                baseElection = JsonConvert.DeserializeObject<BaseElectionModel>(requestBody);
            }
            catch (Exception e)
            {
                _log.LogError("baseElection: invalid format");
                _log.LogDebug("HTTP trigger - CreateElection:End");

                return new BadRequestObjectResult(e.Message);
            }

            _log.LogInformation($"Request Data: {baseElection}");

            var election = new ElectionModel { Name = baseElection.Name, StartDate = baseElection.StartDate, EndDate = baseElection.EndDate };

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Elections.AddAsync(election);
            await _trueVoteDbContext.SaveChangesAsync();

            _log.LogDebug("HTTP trigger - CreateElection:End");

            return new CreatedResult(string.Empty, election);
        }

        [FunctionName(nameof(ElectionFind))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [OpenApiOperation(operationId: "ElectionFind", tags: new[] { "Election" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(FindElectionModel), Description = "Fields to search for Elections", Example = typeof(FindElectionModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ElectionModelList), Description = "Returns collection of Elections")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        public async Task<IActionResult> ElectionFind(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "election/find")] HttpRequest req)
        {
            _log.LogDebug("HTTP trigger - ElectionFind:Begin");

            FindElectionModel findElection;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                findElection = JsonConvert.DeserializeObject<FindElectionModel>(requestBody);
            }
            catch (Exception e)
            {
                _log.LogError("findElection: invalid format");
                _log.LogDebug("HTTP trigger - ElectionFind:End");

                return new BadRequestObjectResult(e.Message);
            }

            _log.LogInformation($"Request Data: {findElection}");

            // TODO Add all the Races to the query
            var items = await _trueVoteDbContext.Elections
                .Where(e =>
                    findElection.Name == null || (e.Name ?? string.Empty).ToLower().Contains(findElection.Name.ToLower()))
                .OrderByDescending(e => e.DateCreated).ToListAsync();

            // TODO Need to respond if not found
            
            _log.LogDebug("HTTP trigger - ElectionFind:End");

            return new OkObjectResult(items);
        }
    }
}

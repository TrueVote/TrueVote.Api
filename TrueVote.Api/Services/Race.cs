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
    public class Race : LoggerHelper
    {
        private readonly TrueVoteDbContext _trueVoteDbContext;

        public Race(ILogger log, TrueVoteDbContext trueVoteDbContext) : base(log)
        {
            _trueVoteDbContext = trueVoteDbContext;
        }

        [FunctionName(nameof(CreateRace))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [OpenApiOperation(operationId: "CreateRace", tags: new[] { "Race" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(BaseRaceModel), Description = "Partially filled Race Model", Example = typeof(BaseRaceModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(RaceModel), Description = "Returns the added race")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnsupportedMediaType, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unsupported Media Type")]
        public async Task<IActionResult> CreateRace(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "race")] HttpRequest req)
        {
            _log.LogDebug("HTTP trigger - CreateRace:Begin");

            BaseRaceModel baseRace;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                baseRace = JsonConvert.DeserializeObject<BaseRaceModel>(requestBody);
            }
            catch (Exception e)
            {
                _log.LogError("baseRace: invalid format");
                _log.LogDebug("HTTP trigger - CreateRace:End");

                return new BadRequestObjectResult(e.Message);
            }

            _log.LogInformation($"Request Data: {baseRace}");

            var race = new RaceModel { Name = baseRace.Name, RaceType = baseRace.RaceType };

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Races.AddAsync(race);
            await _trueVoteDbContext.SaveChangesAsync();

            _log.LogDebug("HTTP trigger - CreateRace:End");

            return new CreatedResult(string.Empty, race);
        }

        [FunctionName(nameof(RaceFind))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [OpenApiOperation(operationId: "RaceFind", tags: new[] { "Race" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(FindRaceModel), Description = "Fields to search for Races", Example = typeof(FindRaceModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RaceModelList), Description = "Returns collection of races")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        public async Task<IActionResult> RaceFind(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "race/find")] HttpRequest req)
        {
            _log.LogDebug("HTTP trigger - RaceFind:Begin");

            FindRaceModel findRace;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                findRace = JsonConvert.DeserializeObject<FindRaceModel>(requestBody);
            }
            catch (Exception e)
            {
                _log.LogError("findRace: invalid format");
                _log.LogDebug("HTTP trigger - RaceFind:End");

                return new BadRequestObjectResult(e.Message);
            }

            _log.LogInformation($"Request Data: {findRace}");

            // TODO Add all the Candidates to the query
            // TODO Fix RaceTypeName not resolving properly
            var items = await _trueVoteDbContext.Races
                .Where(r =>
                    findRace.Name == null || (r.Name ?? string.Empty).ToLower().Contains(findRace.Name.ToLower()))
                .OrderByDescending(r => r.DateCreated).ToListAsync();

            _log.LogDebug("HTTP trigger - RaceFind:End");

            return new OkObjectResult(items);
        }
    }
}

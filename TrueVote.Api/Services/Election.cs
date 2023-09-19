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
    public class Election : LoggerHelper
    {
        private readonly ITrueVoteDbContext _trueVoteDbContext;
        private readonly TelegramBot _telegramBot;

        public Election(ILogger log, ITrueVoteDbContext trueVoteDbContext, TelegramBot telegramBot) : base(log, telegramBot)
        {
            _trueVoteDbContext = trueVoteDbContext;
            _telegramBot = telegramBot;
        }

        [Function(nameof(CreateElection))]
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
        public async Task<HttpResponseData> CreateElection(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "election")] HttpRequestData req)
        {
            LogDebug("HTTP trigger - CreateElection:Begin");

            BaseElectionModel baseElection;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                baseElection = JsonConvert.DeserializeObject<BaseElectionModel>(requestBody);
            }
            catch (Exception e)
            {
                LogError("baseElection: invalid format");
                LogDebug("HTTP trigger - CreateElection:End");

                return await req.CreateBadRequestResponseAsync(new SecureString { Value = e.Message });
            }

            LogInformation($"Request Data: {baseElection}");

            var election = new ElectionModel { Name = baseElection.Name, Description = baseElection.Description, HeaderImageUrl = baseElection.HeaderImageUrl, StartDate = baseElection.StartDate, EndDate = baseElection.EndDate, Races = baseElection.Races };

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Elections.AddAsync(election);
            await _trueVoteDbContext.SaveChangesAsync();

            await _telegramBot.SendChannelMessageAsync($"New TrueVote Election created: {baseElection.Name}");

            LogDebug("HTTP trigger - CreateElection:End");

            return await req.CreateCreatedResponseAsync(election);
        }

        [Function(nameof(ElectionFind))]
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
        public async Task<HttpResponseData> ElectionFind(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "election/find")] HttpRequestData req)
        {
            LogDebug("HTTP trigger - ElectionFind:Begin");

            FindElectionModel findElection;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                findElection = JsonConvert.DeserializeObject<FindElectionModel>(requestBody);
            }
            catch (Exception e)
            {
                LogError("findElection: invalid format");
                LogDebug("HTTP trigger - ElectionFind:End");

                return await req.CreateBadRequestResponseAsync(new SecureString { Value = e.Message });
            }

            LogInformation($"Request Data: {findElection}");

            var items = await _trueVoteDbContext.Elections
                .Where(e =>
                    findElection.Name == null || (e.Name ?? string.Empty).ToLower().Contains(findElection.Name.ToLower()))
                .OrderByDescending(e => e.DateCreated).ToListAsync();

            LogDebug("HTTP trigger - ElectionFind:End");

            return items.Count == 0 ? req.CreateNotFoundResponse() : await req.CreateOkResponseAsync(items);
        }

        [Function(nameof(AddRaces))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [OpenApiOperation(operationId: "AddRaces", tags: new[] { "Election" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(AddRacesModel), Description = "ElectionId and collection of Race Ids", Example = typeof(AddRacesModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(ElectionModel), Description = "Returns the Election")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnsupportedMediaType, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unsupported Media Type")]
        public async Task<HttpResponseData> AddRaces(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "election/addraces")] HttpRequestData req)
        {
            LogDebug("HTTP trigger - AddRaces:Begin");

            AddRacesModel bindRaceElectionModel;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                bindRaceElectionModel = JsonConvert.DeserializeObject<AddRacesModel>(requestBody);
            }
            catch (Exception e)
            {
                LogError("bindRaceElectionModel: invalid format");
                LogDebug("HTTP trigger - AddRaces:End");

                return await req.CreateBadRequestResponseAsync(new SecureString { Value = e.Message });
            }

            LogInformation($"Request Data: {bindRaceElectionModel}");

            // Check if the election exists. If so, return it detached from EF
            var election = await _trueVoteDbContext.Elections.Where(r => r.ElectionId == bindRaceElectionModel.ElectionId).AsNoTracking().OrderByDescending(r => r.DateCreated).FirstOrDefaultAsync();
            if (election == null)
            {
                return await req.CreateNotFoundResponseAsync(new SecureString { Value = $"Election: '{bindRaceElectionModel.ElectionId}' not found" });
            }

            // Check if each Race exists or is already part of the election. If any problems, exit with error
            foreach (var rid in bindRaceElectionModel.RaceIds)
            {
                // Ensure Race exists in Race store
                var race = await _trueVoteDbContext.Races.Where(r => r.RaceId == rid).OrderByDescending(c => c.DateCreated).FirstOrDefaultAsync();
                if (race == null)
                {
                    return await req.CreateNotFoundResponseAsync(new SecureString { Value = $"Race: '{rid}' not found" });
                }

                // Check if it's already part of the Election
                var raceExists = election.Races?.Where(r => r.RaceId == rid).FirstOrDefault();
                if (raceExists != null)
                {
                    return await req.CreateConflictResponseAsync(new SecureString { Value = $"Race: '{rid}' already exists in Election" });
                }

                // Made it this far, add the Race to the Election. Ok to add here because if another one in the list, it won't get persisted
                election.Races.Add(race);
            }

            // If made through the loop of checks above, ok to persist. This will write a new Election
            election.DateCreated = UtcNowProviderFactory.GetProvider().UtcNow;
            election.ElectionId = Guid.NewGuid().ToString();

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Elections.AddAsync(election);
            await _trueVoteDbContext.SaveChangesAsync();

            LogDebug("HTTP trigger - AddRaces:End");

            return await req.CreateCreatedResponseAsync(election);
        }
    }
}

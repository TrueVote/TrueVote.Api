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
    public class User : LoggerHelper
    {
        private readonly TrueVoteDbContext _trueVoteDbContext;

        public User(ILogger log, TrueVoteDbContext trueVoteDbContext) : base(log)
        {
            _trueVoteDbContext = trueVoteDbContext;
        }

        [FunctionName(nameof(CreateUser))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [OpenApiOperation(operationId: "CreateUser", tags: new[] { "User" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(BaseUserModel), Description = "Partially filled User Model", Example = typeof(BaseUserModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(UserModel), Description = "Returns the added User")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnsupportedMediaType, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unsupported Media Type")]
        public async Task<IActionResult> CreateUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user")] HttpRequest req)
        {
            _log.LogDebug("HTTP trigger - CreateUser:Begin");

            BaseUserModel baseUser;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                baseUser = JsonConvert.DeserializeObject<BaseUserModel>(requestBody);
            }
            catch (Exception e)
            {
                _log.LogError("baseUser: invalid format");
                _log.LogDebug("HTTP trigger - CreateUser:End");

                return new BadRequestObjectResult(e.Message);
            }

            _log.LogInformation($"Request Data: {baseUser}");

            var user = new UserModel { FirstName = baseUser.FirstName, Email = baseUser.Email };

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Users.AddAsync(user);
            await _trueVoteDbContext.SaveChangesAsync();

            await TelegramBot.SendChannelMessage("New TrueVote User created");

            _log.LogDebug("HTTP trigger - CreateUser:End");

            return new CreatedResult(string.Empty, user);
        }

        [FunctionName(nameof(UserFind))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [OpenApiOperation(operationId: "UserFind", tags: new[] { "User" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(FindUserModel), Description = "Fields to search for Users", Example = typeof(FindUserModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UserModelList), Description = "Returns collection of Users")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        public async Task<IActionResult> UserFind(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/find")] HttpRequest req)
        {
            _log.LogDebug("HTTP trigger - UserFind:Begin");

            FindUserModel findUser;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                findUser = JsonConvert.DeserializeObject<FindUserModel>(requestBody);
            }
            catch (Exception e)
            {
                _log.LogError("findUser: invalid format");
                _log.LogDebug("HTTP trigger - UserFind:End");

                return new BadRequestObjectResult(e.Message);
            }

            _log.LogInformation($"Request Data: {findUser}");

            // TODO Simplify this query by putting the and conditions in an extension methods to build the where clause more idomatically. It should iterate
            // through all the properties in FindUserModel and build the .Where clause dynamically.
            var items = await _trueVoteDbContext.Users
                .Where(u =>
                    (findUser.FirstName == null || (u.FirstName ?? string.Empty).ToLower().Contains(findUser.FirstName.ToLower())) &&
                    (findUser.Email == null || (u.Email ?? string.Empty).ToLower().Contains(findUser.Email.ToLower())))
                .OrderByDescending(u => u.DateCreated).ToListAsync();

            _log.LogDebug("HTTP trigger - UserFind:End");

            return items.Count == 0 ? new NotFoundResult() : new OkObjectResult(items);
        }
    }
}

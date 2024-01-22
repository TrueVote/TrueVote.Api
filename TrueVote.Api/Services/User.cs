#pragma warning disable IDE0058 // Expression value is never used
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
using Nostr.Client.Keys;
using Nostr.Client.Messages;
using TrueVote.Api.Helpers;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;

namespace TrueVote.Api.Services
{
    public class User : LoggerHelper
    {
        private readonly ITrueVoteDbContext _trueVoteDbContext;
        private readonly IServiceBus _serviceBus;

        public User(ILogger log, ITrueVoteDbContext trueVoteDbContext, IServiceBus serviceBus) : base(log, serviceBus)
        {
            _trueVoteDbContext = trueVoteDbContext;
            _serviceBus = serviceBus;
        }

        [Function(nameof(CreateUser))]
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
        public async Task<HttpResponseData> CreateUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user")] HttpRequestData req)
        {
            LogDebug("HTTP trigger - CreateUser:Begin");

            BaseUserModel baseUser;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                baseUser = JsonConvert.DeserializeObject<BaseUserModel>(requestBody);
            }
            catch (Exception e)
            {
                LogError("baseUser: invalid format");
                LogDebug("HTTP trigger - CreateUser:End");

                return await req.CreateBadRequestResponseAsync(new SecureString { Value = e.Message });
            }

            LogInformation($"Request Data: {baseUser}");

            var user = new UserModel { FirstName = baseUser.FirstName, Email = baseUser.Email };

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Users.AddAsync(user);
            await _trueVoteDbContext.SaveChangesAsync();

            await _serviceBus.SendAsync($"New TrueVote User created: {user.FirstName}");

            LogDebug("HTTP trigger - CreateUser:End");

            return await req.CreateCreatedResponseAsync(user);
        }

        [Function(nameof(UserFind))]
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
        public async Task<HttpResponseData> UserFind(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/find")] HttpRequestData req)
        {
            LogDebug("HTTP trigger - UserFind:Begin");

            FindUserModel findUser;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                findUser = JsonConvert.DeserializeObject<FindUserModel>(requestBody);
            }
            catch (Exception e)
            {
                LogError("findUser: invalid format");
                LogDebug("HTTP trigger - UserFind:End");

                return await req.CreateBadRequestResponseAsync(new SecureString { Value = e.Message });
            }

            LogInformation($"Request Data: {findUser}");

            // TODO Simplify this query by putting the and conditions in an extension methods to build the where clause more idiomatically. It should iterate
            // through all the properties in FindUserModel and build the .Where clause dynamically.
            var items = await _trueVoteDbContext.Users
                .Where(u =>
                    (findUser.FirstName == null || (u.FirstName ?? string.Empty).ToLower().Contains(findUser.FirstName.ToLower())) &&
                    (findUser.Email == null || (u.Email ?? string.Empty).ToLower().Contains(findUser.Email.ToLower())))
                .OrderByDescending(u => u.DateCreated).ToListAsync();

            LogDebug("HTTP trigger - UserFind:End");

            return items.Count == 0 ? req.CreateNotFoundResponse() : await req.CreateOkResponseAsync(items);
        }

        [Function(nameof(SignIn))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [OpenApiOperation(operationId: "SignIn", tags: new[] { "User" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(SignInEventModel), Description = "Fields to search for Users", Example = typeof(SignInEventModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UserModelList), Description = "SignIn Success")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(SecureString), Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(SecureString), Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotAcceptable, contentType: "application/json", bodyType: typeof(SecureString), Description = "Not Acceptable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(SecureString), Description = "Too Many Requests")]
        public async Task<HttpResponseData> SignIn(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/signin")] HttpRequestData req)
        {
            LogDebug("HTTP trigger - SignIn:Begin");

            SignInEventModel signInEventModel;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                signInEventModel = JsonConvert.DeserializeObject<SignInEventModel>(requestBody);
            }
            catch (Exception e)
            {
                LogError("signInEventModel: invalid format");
                LogDebug("HTTP trigger - SignIn:End");

                return await req.CreateBadRequestResponseAsync(new SecureString { Value = e.Message });
            }

            LogInformation($"Request Data: {signInEventModel}");

            NostrPublicKey publicKey;
            try
            {
                publicKey = NostrPublicKey.FromBech32(signInEventModel.PubKey);
            }
            catch (Exception e)
            {
                LogError($"SignIn: publicKey resolver failure: {e.Message}");
                LogDebug("HTTP trigger - SignIn:End");

                return await req.CreateBadRequestResponseAsync(new SecureString { Value = e.Message });
            }

            bool isValid;
            try
            {
                // Create the Nostr Event same as the client did
                var nostrEvent = new NostrEvent
                {
                    Kind = signInEventModel.Kind,
                    CreatedAt = signInEventModel.CreatedAt,
                    Pubkey = publicKey.Hex,
                    Content = signInEventModel.Content,
                    Sig = signInEventModel.Signature
                };

                isValid = nostrEvent.IsSignatureValid();
            }
            catch (Exception e)
            {
                LogError($"SignIn: Verification exception: {e.Message}");
                LogDebug("HTTP trigger - SignIn:End");

                return await req.CreateBadRequestResponseAsync(new SecureString { Value = e.Message });
            }

            if (!isValid)
            {
                LogError("SignIn: invalid signature");
                LogDebug("HTTP trigger - SignIn:End");

                return await req.CreateBadRequestResponseAsync(new SecureString { Value = "Signature did not verify" });
            }

            // TODO - Find the user by PubKey

            // TODO - SignIn the user and return token for API access

            LogDebug("HTTP trigger - SignIn:End");

            return await req.CreateOkResponseAsync(new SecureString { Value = "atoken" });
        }
    }
}
#pragma warning restore IDE0058 // Expression value is never used

using HotChocolate.AzureFunctions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Threading.Tasks;
using TrueVote.Api.Helpers;

namespace TrueVote.Api.Services
{
    public class GraphQLExecutor : LoggerHelper
    {
        private readonly IGraphQLRequestExecutor _graphqlExecutor;

        public GraphQLExecutor(ILogger log, TelegramBot telegramBot, [GraphQL] IGraphQLRequestExecutor executor) : base(log, telegramBot)
        {
            _graphqlExecutor = executor;
        }

        [Function("GraphQL")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [OpenApiOperation(operationId: "GraphQL", tags: new[] { "GraphQL" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(string), Description = "GraphQL Query Entry Point")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Returns GraphQL Query Response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Bad Request")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "graphql/{**slug}")] HttpRequestData req)
        {
            LogDebug("HTTP trigger - GraphQL:Begin");

            var ret = await _graphqlExecutor.ExecuteAsync(req);

            LogDebug("HTTP trigger - GraphQL:End");

            return ret;
        }
    }
}

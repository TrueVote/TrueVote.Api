using HotChocolate.AzureFunctions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Threading.Tasks;
using TrueVote.Api.Helpers;

namespace TrueVote.Api.Services
{
    public class GraphQLFunction : LoggerHelper
    {
        public GraphQLFunction(ILogger log, TelegramBot telegramBot) : base(log, telegramBot)
        {
        }

        [FunctionName("GraphQLHttpFunction")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [OpenApiOperation(operationId: "GraphQLFunction", tags: new[] { "GraphQL" })]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = "https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration", OpenIdConnectScopes = "openid,profile")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(string), Description = "GraphQL Query Entry Point")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Returns GraphQL Query Response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Bad Request")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "graphql/{**slug}")] HttpRequest req, [GraphQL] IGraphQLRequestExecutor executor)
        {
            LogDebug("HTTP trigger - GraphQLHttpFunction:Begin");

            var ret = executor.ExecuteAsync(req);

            LogDebug("HTTP trigger - GraphQLHttpFunction:End");

            return await ret;
        }
    }
}

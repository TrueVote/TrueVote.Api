using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using TrueVote.Api.Models;

namespace TrueVote.Api
{
    public class User
    {
        protected ILogger _log;

        public User(ILogger log)
        {
            _log = log;
        }

        [FunctionName("user")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "User" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(UserModel), Description = "Partially filled User Model", Example = typeof(UserModel))]
        // [OpenApiParameter(name: "user", In = ParameterLocation.Query, Required = true, Type = typeof(User), Description = "User Model")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UserModel), Description = "Returns the status of adding a user")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [CosmosDB(databaseName: "true-vote", collectionName: "users", ConnectionStringSetting = "CosmosDbConnectionString", CreateIfNotExists = true)] IAsyncCollector<dynamic> documentsOut)
        {
            _log.LogDebug("HTTP trigger - User:Begin");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var user = JsonConvert.DeserializeObject<UserModel>(requestBody);
            _log.LogInformation($"Request Data: {user}");

            await documentsOut.AddAsync(new
            {
                user
            });

            _log.LogDebug("HTTP trigger - User:End");

            return new OkResult();
        }
    }
}


using System;
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
using TrueVote.Api.Helpers;
using TrueVote.Api.Models;

namespace TrueVote.Api
{
    public class User : LoggerHelper
    {
        public User(ILogger log): base(log)
        {
        }

        [FunctionName("user")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [OpenApiOperation(operationId: "Run", tags: new[] { "User" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(BaseUserModel), Description = "Partially filled User Model", Example = typeof(BaseUserModel))]
        // [OpenApiParameter(name: "user", In = ParameterLocation.Query, Required = true, Type = typeof(User), Description = "User Model")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(UserModel), Description = "Returns the added user")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [CosmosDB(databaseName: "true-vote", collectionName: "users", ConnectionStringSetting = "CosmosDbConnectionString", CreateIfNotExists = true)] IAsyncCollector<dynamic> documentsOut)
        {
            _log.LogDebug("HTTP trigger - User:Begin");

            BaseUserModel baseUser;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                baseUser = JsonConvert.DeserializeObject<BaseUserModel>(requestBody);
            }
            catch (Exception e)
            {
                _log.LogDebug("HTTP trigger - User:End");
                _log.LogError("baseUser: invalid format");

                return new BadRequestObjectResult(e.Message);
            }

            _log.LogInformation($"Request Data: {baseUser}");

            var user = new UserModel(baseUser);

            await documentsOut.AddAsync(new
            {
                user
            });

            _log.LogDebug("HTTP trigger - User:End");

            return new CreatedResult(string.Empty, user);
        }
    }
}

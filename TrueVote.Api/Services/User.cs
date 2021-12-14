using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
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

        [FunctionName(nameof(CreateUser))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [OpenApiOperation(operationId: "CreateUser", tags: new[] { "User" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(BaseUserModel), Description = "Partially filled User Model", Example = typeof(BaseUserModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(UserModel), Description = "Returns the added user")]
        public async Task<IActionResult> CreateUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user")] HttpRequest req,
            [CosmosDB(databaseName: "true-vote", collectionName: "users", ConnectionStringSetting = "CosmosDbConnectionString", CreateIfNotExists = true)] IAsyncCollector<dynamic> documentsOut)
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

            var user = new UserModel(baseUser);

            await documentsOut.AddAsync(new
            {
                user
            });

            _log.LogDebug("HTTP trigger - CreateUser:End");

            return new CreatedResult(string.Empty, user);
        }

        [FunctionName(nameof(UserFind))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [OpenApiOperation(operationId: "UserFind", tags: new[] { "User" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(FindUserModel), Description = "Fields to search for Users", Example = typeof(FindUserModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<UserModel>), Description = "Returns collection of users")]
        public async Task<IActionResult> UserFind(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/find")] HttpRequest req,
            [CosmosDB(databaseName: "true-vote", collectionName: "users", ConnectionStringSetting = "CosmosDbConnectionString", CreateIfNotExists = true)] DocumentClient client)
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

            var driverCollectionUri = UriFactory.CreateDocumentCollectionUri(databaseId: "true-vote", collectionId: "users");
            var options = new FeedOptions { EnableCrossPartitionQuery = true };

            var userList = client.CreateDocumentQuery<UserModel>(driverCollectionUri, options).Where(u => u.FirstName.Contains(findUser.FirstName)).AsDocumentQuery();

            var userListReturn = new List<UserModel>();
            while (userList.HasMoreResults)
            {
                foreach (UserModel user in await userList.ExecuteNextAsync())
                {
                    userListReturn.Add(user);
                }
            }
            _log.LogDebug("HTTP trigger - UserFind:End");

            return new OkObjectResult(userListReturn);
        }
    }
}

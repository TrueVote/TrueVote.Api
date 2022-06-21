using HotChocolate.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TrueVote.Api.Models;
using TrueVote.Api.Services;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;
using MockQueryable.Moq;
using Microsoft.Extensions.DependencyInjection;
using HotChocolate.AzureFunctions;
using Microsoft.AspNetCore.Http;
using System.Data.Entity.Core.Objects;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using System.Net.Http;
using HotChocolate.Execution;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class GraphQLTest : TestHelper
    {
        // private readonly IGraphQLRequestExecutor requestExecutor;
        private readonly HttpContext httpContext;
        private readonly HttpClient httpClient;
        private byte[] buffer;

        public GraphQLTest(ITestOutputHelper output) : base(output)
        {
            httpContext = new DefaultHttpContext();
            httpClient = new HttpClient();
        }

        private static ActionContext PrepareActionContext()
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }

        [Fact]
        public async Task RunsCandidateQuery()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGraphQLFunction().AddQueryType<Query>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var requestExecutor = serviceProvider.GetRequiredService<IGraphQLRequestExecutor>();

            var graphQLQuery = "{ candidates { name, partyAffiliation } }";

            var graphQLRequestObj = $"{{\"query\":\"{graphQLQuery}\"}}";

            var byteArray = Encoding.ASCII.GetBytes(graphQLRequestObj);

            httpContext.Request.Body = new MemoryStream(byteArray);
            httpContext.Request.ContentType = "application/json";
            httpContext.Request.ContentLength = _httpContext.Request.Body.Length;
            httpContext.Request.Path = "/api/graphql";
            httpContext.Request.Protocol = "HTTP/1.1";
            httpContext.Request.Method = "POST";

            var ret = await _graphQLApi.Run(httpContext.Request, requestExecutor);
            Assert.NotNull(ret);

            var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();

            var baseCandidate = JsonConvert.DeserializeObject(responseBody);
            var t = Task.FromResult(ret);

            // var responseBody = new StreamReader(body).ReadToEnd();

            // var objectResult = Assert.IsType<CandidateModel>(httpContext.Response.Body);

            //Assert.NotEmpty(val);
            //Assert.Single(val);
            //Assert.Equal("John Smith", val[0].Name);
            Assert.Equal((int) HttpStatusCode.OK, httpContext.Response.StatusCode);

            logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        // TODO Add Get() tests for all Services
    }
}

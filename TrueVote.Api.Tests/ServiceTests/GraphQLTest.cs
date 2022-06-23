using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TrueVote.Api.Services;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using HotChocolate.AzureFunctions;
using Microsoft.AspNetCore.Http;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class GraphQLTest : TestHelper
    {
        private readonly HttpContext httpContext;

        public GraphQLTest(ITestOutputHelper output) : base(output)
        {
            httpContext = new DefaultHttpContext();
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

            var ret = await _graphQLApi.Run(httpContext.Request, requestExecutor);
            Assert.NotNull(ret);

            // TODO Assert() statements for actual data returned. Right now, it returns an empty result
            // The challenge is how to get the BODY from the response off of the context.
            // This query works in Postman, Banana Cake Pop, but in those environments, it's called using
            // an actual http pipeline context. Here on line 43 it's directly calling the service without
            // a fully materialized context.
            //
            // HotChocolate says that it's returned in the response stream here:
            // https://github.com/ChilliCream/hotchocolate/blob/c2e8bbc0a9c7dc5da3ed2ffb6b669ee533d75d75/src/HotChocolate/AzureFunctions/src/HotChocolate.AzureFunctions/DefaultGraphQLRequestExecutor.cs#L32

            // Ticket
            // https://truevote.atlassian.net/browse/AD-39
            // https://github.com/TrueVote/TrueVote.Api/issues/19

            Assert.Equal((int) HttpStatusCode.OK, httpContext.Response.StatusCode);

            logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        // TODO Add Get() tests for all Services
    }
}

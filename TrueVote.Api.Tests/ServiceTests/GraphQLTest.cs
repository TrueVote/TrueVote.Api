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

namespace TrueVote.Api.Tests.ServiceTests
{
    public class GraphQLTest : TestHelper
    {
        // private readonly IGraphQLRequestExecutor requestExecutor;

        public GraphQLTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task RunsCandidateQuery()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGraphQLFunction().AddQueryType<Query>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var requestExecutor = serviceProvider.GetRequiredService<IGraphQLRequestExecutor>();

            var graphQLRequestObj = "{ candidate { name } }";

            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(graphQLRequestObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            // TODO Figure out how to moq GraphQL
            var ret = await _graphQLApi.Run(_httpContext.Request, requestExecutor);
            Assert.NotNull(ret);

            logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        // TODO Add Get() tests for all Services
    }
}

using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using TrueVote.Api.Models;
using System.Collections.Generic;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class GraphQLTest : TestHelper
    {
        public static readonly string GraphQLRootFormat = "{{\"query\":\"{0}\"}}";
        public static readonly string GraphQLResponseRootHeader = "{\"data\":{\"candidate\":";

        public GraphQLTest(ITestOutputHelper output) : base(output)
        {
            _httpContext.Request.ContentType = "application/json";
            _httpContext.Request.Path = "/api/graphql";
            _httpContext.Request.Method = "POST";
        }

        [Fact]
        public async Task RunsCandidateQuery()
        {
            var graphQLQuery = "{ candidate { candidateId, name, partyAffiliation } }";
            var graphQLRequestObj = string.Format(GraphQLRootFormat, graphQLQuery);

            var byteArray = Encoding.ASCII.GetBytes(graphQLRequestObj);
            _httpContext.Request.Body = new MemoryStream(byteArray);
            _httpContext.Request.ContentLength = _httpContext.Request.Body.Length;

            var ret = await _graphQLApi.Run(_httpContext.Request, requestExecutor);
            Assert.NotNull(ret);

            var responseStream = _httpContext.Response.Body as MemoryStream;
            var responseBody = Encoding.ASCII.GetString(responseStream.ToArray());
            Assert.StartsWith(GraphQLResponseRootHeader, responseBody);

            var graphQLRoot = JsonConvert.DeserializeObject<GraphQLCandidateRoot>(responseBody).Data;
            var candidates = JsonConvert.DeserializeObject<List<CandidateModel>>(JsonConvert.SerializeObject(graphQLRoot.candidate));
            Assert.Equal("Jane Doe", candidates[0].Name);
            Assert.Equal("John Smith", candidates[1].Name);
            Assert.True(candidates.Count == 2);

            Assert.Equal((int) HttpStatusCode.OK, _httpContext.Response.StatusCode);
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        // TODO Add Get() tests for all Services
    }
}

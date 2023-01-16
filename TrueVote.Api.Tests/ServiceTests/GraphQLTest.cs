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
    public class GraphQLRoot
    {
        [JsonProperty(PropertyName = "Data")]
        public dynamic Data { get; set; }
    }

    public class GraphQLTest : TestHelper
    {
        public static readonly string GraphQLRootFormat = "{{\"query\":\"{0}\"}}";
        public static readonly string GraphQLResponseRootHeader = "{\"data\":";

        public GraphQLTest(ITestOutputHelper output) : base(output)
        {
            _httpContext.Request.Path = "/api/graphql";
            _httpContext.Request.Method = "POST";
            // _httpContext.Response.ContentType = "application/graphql";
        }

        // Takes a GraphQL query packages it, sends it, and checks for valid response
        private async Task<dynamic> GraphQLQuerySetup(string graphQLQuery)
        {
            var graphQLRequestObj = string.Format(GraphQLRootFormat, graphQLQuery);

            var byteArray = Encoding.ASCII.GetBytes(graphQLRequestObj);
            _httpContext.Request.Body = new MemoryStream(byteArray);
            _httpContext.Request.ContentLength = _httpContext.Request.Body.Length;

            var ret = await _graphQLApi.Run(_httpContext.Request, requestExecutor);
            Assert.NotNull(ret);

            var responseStream = _httpContext.Response.Body as MemoryStream;
            var responseBody = Encoding.ASCII.GetString(responseStream.ToArray());
            Assert.StartsWith(GraphQLResponseRootHeader, responseBody);

            var graphQLRoot = JsonConvert.DeserializeObject<GraphQLRoot>(responseBody).Data;

            return graphQLRoot;
        }

        [Fact]
        public async Task RunsCandidateQuery()
        {
            var graphQLRoot = await GraphQLQuerySetup("{ GetCandidate { CandidateId, Name, PartyAffiliation } }");

            var candidates = JsonConvert.DeserializeObject<List<CandidateModel>>(JsonConvert.SerializeObject(graphQLRoot.GetCandidate));
            Assert.Equal("Jane Doe", candidates[0].Name);
            Assert.Equal("John Smith", candidates[1].Name);
            Assert.True(candidates.Count == 2);

            Assert.Equal((int) HttpStatusCode.OK, _httpContext.Response.StatusCode);
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task RunsElectionQuery()
        {
            var graphQLRoot = await GraphQLQuerySetup("{ GetElection { ElectionId, Name, DateCreated } }");

            var elections = JsonConvert.DeserializeObject<List<ElectionModelReponse>>(JsonConvert.SerializeObject(graphQLRoot.GetElection));
            Assert.Equal("Federal", elections[0].Name);
            Assert.Equal("Los Angeles County", elections[1].Name);
            Assert.True(elections.Count == 3);

            Assert.Equal((int) HttpStatusCode.OK, _httpContext.Response.StatusCode);
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task RunsElectionByIdQuery()
        {
            var id = "7d9c6d23-9534-40d4-bca3-9769d0a54d76";

            var graphQLRoot = await GraphQLQuerySetup($"{{ GetElectionById ($ElectionId:\"{id}\") {{ ElectionId, Name, DateCreated }} }}");

            var elections = JsonConvert.DeserializeObject<List<ElectionModelReponse>>(JsonConvert.SerializeObject(graphQLRoot.election));
            Assert.Equal("Federal", elections[0].Name);
            Assert.Equal("Los Angeles County", elections[1].Name);
            Assert.True(elections.Count == 3);

            Assert.Equal((int) HttpStatusCode.OK, _httpContext.Response.StatusCode);
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task RunsRaceQuery()
        {
            var graphQLRoot = await GraphQLQuerySetup("{ GetRace { DateCreated, Name, RaceId, RaceType, RaceTypeName, Candidates { CandidateId, Name, PartyAffiliation, DateCreated } } }");

            var race = JsonConvert.DeserializeObject<List<RaceModelResponse>>(JsonConvert.SerializeObject(graphQLRoot.GetRace));
            Assert.Equal("Governor", race[0].Name);
            Assert.Equal(RaceTypes.ChooseOne, race[0].RaceType);
            Assert.Equal("ChooseOne", race[0].RaceTypeName);
            Assert.Equal("Judge", race[1].Name);
            Assert.Equal(RaceTypes.ChooseMany, race[1].RaceType);
            Assert.Equal("ChooseMany", race[1].RaceTypeName);
            Assert.True(race.Count == 3);

            Assert.Equal((int) HttpStatusCode.OK, _httpContext.Response.StatusCode);
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task RunsUserQuery()
        {
            var graphQLRoot = await GraphQLQuerySetup("{ GetUser { DateCreated, Email, FirstName, UserId } }");

            var race = JsonConvert.DeserializeObject<List<UserModel>>(JsonConvert.SerializeObject(graphQLRoot.GetUser));
            Assert.Equal("Boo", race[0].FirstName);
            Assert.Equal("Foo2", race[1].FirstName);
            Assert.True(race.Count == 3);

            Assert.Equal((int) HttpStatusCode.OK, _httpContext.Response.StatusCode);
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

    }
}

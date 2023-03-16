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
using GraphQL;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class GraphQLRoot
    {
        [JsonProperty(PropertyName = "Data")]
        public dynamic Data { get; set; }
    }

    public class GraphQLTest : TestHelper
    {
        public GraphQLTest(ITestOutputHelper output) : base(output)
        {
            _httpContext.Request.Path = "/api/graphql";
            _httpContext.Request.Method = "POST";
            // _httpContext.Response.ContentType = "application/graphql";
        }

        // Takes a GraphQL query packages it, sends it, and checks for valid response
        private async Task<dynamic> GraphQLQuerySetup(string graphQLQuery)
        {
            var byteArray = Encoding.ASCII.GetBytes(graphQLQuery);
            _httpContext.Request.Body = new MemoryStream(byteArray);
            _httpContext.Request.ContentLength = _httpContext.Request.Body.Length;

            var ret = await _graphQLApi.Run(_httpContext.Request, requestExecutor);
            Assert.NotNull(ret);

            var responseStream = _httpContext.Response.Body as MemoryStream;
            var responseBody = Encoding.ASCII.GetString(responseStream.ToArray());
            var graphQLResponseRootHeader = "{\"data\":";
            Assert.StartsWith(graphQLResponseRootHeader, responseBody);

            var graphQLRoot = JsonConvert.DeserializeObject<GraphQLRoot>(responseBody).Data;

            return graphQLRoot;
        }

        [Fact]
        public async Task RunsCandidateQuery()
        {
            var graphQLRequest = new GraphQLRequest
            {
                Query = @"{ GetCandidate { CandidateId, Name, PartyAffiliation } }"
            };

            var graphQLRequestJson = JsonConvert.SerializeObject(graphQLRequest);
            var graphQLRoot = await GraphQLQuerySetup(graphQLRequestJson);

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
            var graphQLRequest = new GraphQLRequest
            {
                Query = @"{ GetElection { ElectionId, Name, DateCreated } }"
            };

            var graphQLRequestJson = JsonConvert.SerializeObject(graphQLRequest);
            var graphQLRoot = await GraphQLQuerySetup(graphQLRequestJson);

            var elections = JsonConvert.DeserializeObject<List<ElectionModelResponse>>(JsonConvert.SerializeObject(graphQLRoot.GetElection));
            Assert.Equal("Federal", elections[0].Name);
            Assert.Equal("Los Angeles County", elections[1].Name);
            Assert.True(elections.Count == 3);

            Assert.Equal((int) HttpStatusCode.OK, _httpContext.Response.StatusCode);
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task RunsElectionByIdQuery()
        {
            var electionId = "68";

            var graphQLRequest = new GraphQLRequest
            {
                Query = @"
                    query ($ElectionId: String!) {
                        GetElectionById(ElectionId: $ElectionId) { ElectionId, Name, DateCreated }
                    }",
                Variables = new
                {
                    ElectionId = electionId
                }
            };

            var graphQLRequestJson = JsonConvert.SerializeObject(graphQLRequest);

            var graphQLRoot = await GraphQLQuerySetup(graphQLRequestJson);

            var elections = JsonConvert.DeserializeObject<List<ElectionModelResponse>>(JsonConvert.SerializeObject(graphQLRoot.GetElectionById));
            Assert.NotNull(elections);
            Assert.Equal("Federal", elections[0].Name);
            Assert.Equal("68", elections[0].ElectionId);
            Assert.True(elections.Count == 1);

            Assert.Equal((int) HttpStatusCode.OK, _httpContext.Response.StatusCode);
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task RunsRaceQuery()
        {
            var graphQLRequest = new GraphQLRequest
            {
                Query = @"{ GetRace { DateCreated, Name, RaceId, RaceType, RaceTypeName, Candidates { CandidateId, Name, PartyAffiliation, DateCreated } } }"
            };

            var graphQLRequestJson = JsonConvert.SerializeObject(graphQLRequest);
            var graphQLRoot = await GraphQLQuerySetup(graphQLRequestJson);

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
            var graphQLRequest = new GraphQLRequest
            {
                Query = @"{ GetUser { DateCreated, Email, FirstName, UserId } }"
            };

            var graphQLRequestJson = JsonConvert.SerializeObject(graphQLRequest);
            var graphQLRoot = await GraphQLQuerySetup(graphQLRequestJson);

            var race = JsonConvert.DeserializeObject<List<UserModel>>(JsonConvert.SerializeObject(graphQLRoot.GetUser));
            Assert.Equal("Boo", race[0].FirstName);
            Assert.Equal("Foo2", race[1].FirstName);
            Assert.True(race.Count == 3);

            Assert.Equal((int) HttpStatusCode.OK, _httpContext.Response.StatusCode);
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

    }
}

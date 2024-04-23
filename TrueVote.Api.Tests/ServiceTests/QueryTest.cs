using System.Threading.Tasks;
using TrueVote.Api.Models;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class QueryTest : TestHelper
    {
        public QueryTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task RunsCandidateQuery()
        {
            var ret = await _queryService.GetCandidate();

            Assert.NotNull(ret);
            Assert.Equal("Jane Doe", ret[0].Name);
            Assert.Equal("John Smith", ret[1].Name);
            Assert.True(ret.Count == 2);
        }

        [Fact]
        public async Task RunsCandidateByPartyAffiliationQuery()
        {
            var partyAffiliation = "Republican";

            var ret = await _queryService.GetCandidateByPartyAffiliation(partyAffiliation);

            Assert.NotNull(ret);
            Assert.Equal("John Smith", ret[0].Name);
            Assert.True(ret.Count == 1);
        }

        [Fact]
        public async Task RunsElectionQuery()
        {
            var ret = await _queryService.GetElection();

            Assert.NotNull(ret);
            Assert.Equal("Federal", ret[0].Name);
            Assert.Equal("Los Angeles County", ret[1].Name);
            Assert.True(ret.Count == 3);
        }

        [Fact]
        public async Task RunsElectionByIdQuery()
        {
            var electionId = "electionid3";

            var ret = await _queryService.GetElectionById(electionId);

            Assert.NotNull(ret);
            Assert.Equal("Federal", ret[0].Name);
            Assert.Equal("electionid3", ret[0].ElectionId);
            Assert.True(ret.Count == 1);
        }

        [Fact]
        public async Task RunsRaceQuery()
        {
            var ret = await _queryService.GetRace();

            Assert.NotNull(ret);
            Assert.Equal("Governor", ret[0].Name);
            Assert.Equal(RaceTypes.ChooseOne, ret[0].RaceType);
            Assert.Equal("ChooseOne", ret[0].RaceTypeName);
            Assert.Equal("Judge", ret[1].Name);
            Assert.Equal(RaceTypes.ChooseMany, ret[1].RaceType);
            Assert.Equal("ChooseMany", ret[1].RaceTypeName);
            Assert.True(ret.Count == 3);
        }

        [Fact]
        public async Task RunsUserQuery()
        {
            var ret = await _queryService.GetUser();

            Assert.NotNull(ret);
            Assert.Equal("Boo Bar", ret[0].FullName);
            Assert.Equal("Foo2 Bar", ret[1].FullName);
            Assert.True(ret.Count == 3);
        }

        [Fact]
        public async Task RunsBallotQuery()
        {
            var ret = await _queryService.GetBallot();

            Assert.NotNull(ret);
            Assert.Equal("electionid1", ret.Ballots[0].Election.ElectionId);
            Assert.Equal("ballotid3", ret.Ballots[0].BallotId);
            Assert.True(ret.Ballots.Count == 3);
        }

        [Fact]
        public async Task RunsBallotByIdQuery()
        {
            var ballotId = "ballotid3";

            var ret = await _queryService.GetBallotById(ballotId);

            Assert.NotNull(ret);
            Assert.Equal("electionid1", ret.Ballots[0].Election.ElectionId);
            Assert.Equal("ballotid3", ret.Ballots[0].BallotId);
            Assert.True(ret.Ballots.Count == 1);
        }
    }
}

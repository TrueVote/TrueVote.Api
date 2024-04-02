using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueVote.Api.Models;
using TrueVote.Api.Services;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;
using static TrueVote.Api.Startup;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class FakeBaseElectionModel
    {
        public string Name { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ElectionTest : TestHelper
    {
        public ElectionTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task LogsMessages()
        {
            var baseElectionObj = new BaseElectionModel { Name = "California State", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Description = "desc", HeaderImageUrl = "url", Races = [] };

            await _electionApi.CreateElection(baseElectionObj);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsElection()
        {
            var baseElectionObj = new BaseElectionModel { Name = "California State", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Description = "desc", HeaderImageUrl = "url", Races = [] };

            var ret = await _electionApi.CreateElection(baseElectionObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status201Created, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (ElectionModel) (ret as CreatedAtActionResult).Value;
            Assert.NotNull(val);

            _output.WriteLine($"Item: {val}");

            _output.WriteLine($"Item.Name: {val.Name}");
            _output.WriteLine($"Item.StartDate: {val.StartDate}");
            _output.WriteLine($"Item.EndDate: {val.EndDate}");
            _output.WriteLine($"Item.DateCreated: {val.DateCreated}");
            _output.WriteLine($"Item.ElectionId: {val.ElectionId}");

            Assert.Equal("California State", val.Name);
            Assert.IsType<DateTime>(val.DateCreated);
            Assert.NotEmpty(val.ElectionId);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public void FindsElection()
        {
            var findElectionObj = new FindElectionModel { Name = "County" };

            var electionApi = new Election(_logHelper.Object, _moqDataAccessor.mockElectionContext.Object, _mockServiceBus.Object);

            var ret = electionApi.ElectionFind(findElectionObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (ElectionModelList) (ret as OkObjectResult).Value;
            Assert.NotEmpty(val.Elections);
            Assert.Single(val.Elections);
            Assert.Equal("Los Angeles County", val.Elections[0].Name);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public void HandlesUnfoundElection()
        {
            var findElectionObj = new FindElectionModel { Name = "not going to find anything" };

            var electionApi = new Election(_logHelper.Object, _moqDataAccessor.mockElectionContext.Object, _mockServiceBus.Object);

            var ret = electionApi.ElectionFind(findElectionObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsRacesToElection()
        {
            var addsRacesElectionData = MoqData.MockElectionData;

            addsRacesElectionData[0].ElectionId = "1";
            addsRacesElectionData[1].ElectionId = "2";

            // https://docs.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking?redirectedfrom=MSDN
            // https://github.com/romantitov/MockQueryable
            var mockElectionContext = new Mock<TrueVoteDbContext>();

            var mockElectionSet = addsRacesElectionData.AsQueryable().BuildMockDbSet();
            mockElectionContext.Setup(m => m.Elections).Returns(mockElectionSet.Object);

            var mockRacesSet = _moqDataAccessor.mockRaceDataCollection.AsQueryable().BuildMockDbSet();
            mockElectionContext.Setup(m => m.Races).Returns(mockRacesSet.Object);

            var addRacesObj = new AddRacesModel { ElectionId = "1", RaceIds = new List<string> { MoqData.MockRaceData[0].RaceId, MoqData.MockRaceData[1].RaceId, MoqData.MockRaceData[2].RaceId } };

            var electionApi = new Election(_logHelper.Object, mockElectionContext.Object, _mockServiceBus.Object);

            var ret = await electionApi.AddRaces(addRacesObj);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status201Created, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (ElectionModel) (ret as CreatedAtActionResult).Value;
            Assert.NotNull(val);
            Assert.Equal("California State", val.Name);
            Assert.Equal("President", val.Races.ToList()[0].Name);
            Assert.Equal("Judge", val.Races.ToList()[1].Name);
            Assert.Equal("Governor", val.Races.ToList()[2].Name);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesAddRacesUnfoundElection()
        {
            var addsRacesElectionData = MoqData.MockElectionData;

            addsRacesElectionData[0].ElectionId = "1";
            addsRacesElectionData[1].ElectionId = "2";

            var mockElectionContext = new Mock<TrueVoteDbContext>();

            var mockElectionSet = addsRacesElectionData.AsQueryable().BuildMockDbSet();
            mockElectionContext.Setup(m => m.Elections).Returns(mockElectionSet.Object);

            var addRacesObj = new AddRacesModel { ElectionId = "blah", RaceIds = new List<string>() { } };

            var electionApi = new Election(_logHelper.Object, mockElectionContext.Object, _mockServiceBus.Object);

            var ret = await electionApi.AddRaces(addRacesObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as NotFoundObjectResult).Value;
            Assert.Contains("Election", val.Value.ToString());
            Assert.Contains("not found", val.Value.ToString());

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(1));
        }

        [Fact]
        public async Task HandlesAddRacesUnfoundRace()
        {
            var addsRacesElectionData = MoqData.MockElectionData;

            addsRacesElectionData[0].ElectionId = "1";
            addsRacesElectionData[1].ElectionId = "2";

            var mockElectionContext = new Mock<TrueVoteDbContext>();

            var mockElectionSet = addsRacesElectionData.AsQueryable().BuildMockDbSet();
            mockElectionContext.Setup(m => m.Elections).Returns(mockElectionSet.Object);

            var mockRacesSet = _moqDataAccessor.mockRaceDataCollection.AsQueryable().BuildMockDbSet();
            mockElectionContext.Setup(m => m.Races).Returns(mockRacesSet.Object);

            var addRacesObj = new AddRacesModel { ElectionId = "1", RaceIds = new List<string> { "68", "69", "70" } };

            var electionApi = new Election(_logHelper.Object, mockElectionContext.Object, _mockServiceBus.Object);

            var ret = await electionApi.AddRaces(addRacesObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as NotFoundObjectResult).Value;
            Assert.Contains("Race", val.Value.ToString());
            Assert.Contains("not found", val.Value.ToString());

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(1));
        }

        [Fact]
        public async Task HandlesAddRaceAlreadyInElection()
        {
            var addsRacesElectionData = MoqData.MockElectionData;

            addsRacesElectionData[0].Races = _moqDataAccessor.mockRaceDataCollection;
            addsRacesElectionData[0].ElectionId = "electionid1";
            addsRacesElectionData[1].ElectionId = "electionid2";

            var mockElectionContext = new Mock<TrueVoteDbContext>();

            var mockElectionSet = addsRacesElectionData.AsQueryable().BuildMockDbSet();
            mockElectionContext.Setup(m => m.Elections).Returns(mockElectionSet.Object);

            var mockRacesSet = _moqDataAccessor.mockRaceDataCollection.AsQueryable().BuildMockDbSet();
            mockElectionContext.Setup(m => m.Races).Returns(mockRacesSet.Object);

            var addRacesObj = new AddRacesModel { ElectionId = "electionid1", RaceIds = new List<string> { "raceid1", "raceid2", "raceid3" } };

            var electionApi = new Election(_logHelper.Object, mockElectionContext.Object, _mockServiceBus.Object);

            var ret = await electionApi.AddRaces(addRacesObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status409Conflict, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as ConflictObjectResult).Value;
            Assert.Contains("Race", val.Value.ToString());
            Assert.Contains("already exists", val.Value.ToString());

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(1));
        }
    }
}

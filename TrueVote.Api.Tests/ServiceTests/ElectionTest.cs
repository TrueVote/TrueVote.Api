using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TrueVote.Api.Models;
using TrueVote.Api.Services;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

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
            var baseElectionObj = new BaseElectionModel { Name = "California State", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(baseElectionObj));

            _ = await _electionApi.CreateElection(requestData);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsElection()
        {
            var baseElectionObj = new BaseElectionModel { Name = "California State", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(baseElectionObj));

            var ret = await _electionApi.CreateElection(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.Created, ret.StatusCode);

            var val = await ret.ReadAsJsonAsync<ElectionModel>();
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
        public async Task HandlesInvalidElectionCreate()
        {
            // This object is missing required property (StartDate)
            var fakeBaseElectionObj = new FakeBaseElectionModel { Name = "California State" };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(fakeBaseElectionObj));

            var ret = await _electionApi.CreateElection(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.BadRequest, ret.StatusCode);
            var val = await ret.ReadAsJsonAsync<SecureString>();
            Assert.Contains("Required", val.Value.ToString());

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task FindsElection()
        {
            var findElectionObj = new FindElectionModel { Name = "County" };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(findElectionObj));

            var electionApi = new Election(_logHelper.Object, _moqDataAccessor.mockElectionContext.Object, _mockServiceBus.Object);

            var ret = await electionApi.ElectionFind(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.OK, ret.StatusCode);

            var val = await ret.ReadAsJsonAsync<List<ElectionModel>>();
            Assert.NotEmpty(val);
            Assert.Single(val);
            Assert.Equal("Los Angeles County", val[0].Name);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundElection()
        {
            var findElectionObj = new FindElectionModel { Name = "not going to find anything" };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(findElectionObj));

            var electionApi = new Election(_logHelper.Object, _moqDataAccessor.mockElectionContext.Object, _mockServiceBus.Object);

            var ret = await electionApi.ElectionFind(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.NotFound, ret.StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesFindElectionError()
        {
            var findElectionObj = "blah";
            var requestData = new MockHttpRequestData(findElectionObj);

            var ret = await _electionApi.ElectionFind(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.BadRequest, ret.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
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
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(addRacesObj));

            var electionApi = new Election(_logHelper.Object, mockElectionContext.Object, _mockServiceBus.Object);

            var ret = await electionApi.AddRaces(requestData);

            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.Created, ret.StatusCode);

            var val = await ret.ReadAsJsonAsync<ElectionModel>();
            Assert.NotNull(val);
            Assert.Equal("California State", val.Name);
            Assert.Equal("President", val.Races.ToList()[0].Name);
            Assert.Equal("Judge", val.Races.ToList()[1].Name);
            Assert.Equal("Governor", val.Races.ToList()[2].Name);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesAddRacesError()
        {
            var addRacesObj = "blah";
            var requestData = new MockHttpRequestData(addRacesObj);

            var ret = await _electionApi.AddRaces(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.BadRequest, ret.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
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
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(addRacesObj));

            var electionApi = new Election(_logHelper.Object, mockElectionContext.Object, _mockServiceBus.Object);

            var ret = await electionApi.AddRaces(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.NotFound, ret.StatusCode);
            var val = await ret.ReadAsJsonAsync<SecureString>();
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
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(addRacesObj));

            var electionApi = new Election(_logHelper.Object, mockElectionContext.Object, _mockServiceBus.Object);

            var ret = await electionApi.AddRaces(requestData);

            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.NotFound, ret.StatusCode);
            var val = await ret.ReadAsJsonAsync<SecureString>();
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
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(addRacesObj));

            var electionApi = new Election(_logHelper.Object, mockElectionContext.Object, _mockServiceBus.Object);

            var ret = await electionApi.AddRaces(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.Conflict, ret.StatusCode);
            var val = await ret.ReadAsJsonAsync<SecureString>();
            Assert.Contains("Race", val.Value.ToString());
            Assert.Contains("already exists", val.Value.ToString());

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(1));
        }
    }
}

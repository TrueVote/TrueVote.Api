using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
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
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(baseElectionObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            _ = await _electionApi.CreateElection(_httpContext.Request);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsElection()
        {
            var baseElectionObj = new BaseElectionModel { Name = "California State", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(baseElectionObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _electionApi.CreateElection(_httpContext.Request) as CreatedResult;
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<CreatedResult>(ret);
            Assert.Equal((int) HttpStatusCode.Created, objectResult.StatusCode);

            var val = ret.Value as ElectionModel;
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
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(fakeBaseElectionObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _electionApi.CreateElection(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);
            Assert.Contains("Required", objectResult.Value.ToString());

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task FindsElection()
        {
            var findElectionObj = new FindElectionModel { Name = "County" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findElectionObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var electionApi = new Election(_logHelper.Object, _moqDataAccessor.mockElectionContext.Object, _mockTelegram.Object);

            var ret = await electionApi.ElectionFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<OkObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.OK, objectResult.StatusCode);

            var val = objectResult.Value as List<ElectionModel>;
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
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findElectionObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var electionApi = new Election(_logHelper.Object, _moqDataAccessor.mockElectionContext.Object, _mockTelegram.Object);

            var ret = await electionApi.ElectionFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<NotFoundResult>(ret);
            Assert.Equal((int) HttpStatusCode.NotFound, objectResult.StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesFindElectionError()
        {
            var findElectionObj = "blah";
            var byteArray = Encoding.ASCII.GetBytes(findElectionObj);
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _electionApi.ElectionFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);

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
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(addRacesObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var electionApi = new Election(_logHelper.Object, mockElectionContext.Object, _mockTelegram.Object);

            var ret = await electionApi.AddRaces(_httpContext.Request);

            Assert.NotNull(ret);
            var objectResult = Assert.IsType<CreatedResult>(ret);
            Assert.Equal((int) HttpStatusCode.Created, objectResult.StatusCode);

            var val = objectResult.Value as ElectionModel;
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
            var byteArray = Encoding.ASCII.GetBytes(addRacesObj);
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _electionApi.AddRaces(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);

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
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(addRacesObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var electionApi = new Election(_logHelper.Object, mockElectionContext.Object, _mockTelegram.Object);

            var ret = await electionApi.AddRaces(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<NotFoundObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.NotFound, objectResult.StatusCode);
            Assert.Contains("Election", objectResult.Value.ToString());
            Assert.Contains("not found", objectResult.Value.ToString());

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
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(addRacesObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var electionApi = new Election(_logHelper.Object, mockElectionContext.Object, _mockTelegram.Object);

            var ret = await electionApi.AddRaces(_httpContext.Request);

            Assert.NotNull(ret);
            var objectResult = Assert.IsType<NotFoundObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.NotFound, objectResult.StatusCode);
            Assert.Contains("Race", objectResult.Value.ToString());
            Assert.Contains("not found", objectResult.Value.ToString());

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
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(addRacesObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var electionApi = new Election(_logHelper.Object, mockElectionContext.Object, _mockTelegram.Object);

            var ret = await electionApi.AddRaces(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<ConflictObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.Conflict, objectResult.StatusCode);
            Assert.Contains("Race", objectResult.Value.ToString());
            Assert.Contains("already exists", objectResult.Value.ToString());

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(1));
        }
    }
}

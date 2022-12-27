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

namespace TrueVote.Api.Tests.ServiceTests
{
    public class FakeBaseRaceModel
    {
        public string Name { get; set; } = string.Empty;
    }

    public class FakeRaceModel
    {
        public string RaceId { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public RaceTypes RaceType { get; set; }
        public ICollection<CandidateModel> Candidates { get; set; } = new List<CandidateModel>();
    }

    public class RaceTest : TestHelper
    {
        public RaceTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task LogsMessages()
        {
            var baseRaceObj = new BaseRaceModel { Name = "President", RaceType = RaceTypes.ChooseOne };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(baseRaceObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            _ = await _raceApi.CreateRace(_httpContext.Request);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsRace()
        {
            var baseRaceObj = new BaseRaceModel { Name = "President", RaceType = RaceTypes.ChooseOne };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(baseRaceObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _raceApi.CreateRace(_httpContext.Request) as CreatedResult;
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<CreatedResult>(ret);
            Assert.Equal((int) HttpStatusCode.Created, objectResult.StatusCode);

            var val = ret.Value as RaceModel;
            Assert.NotNull(val);

            _output.WriteLine($"Item: {val}");

            _output.WriteLine($"Item.Name: {val.Name}");
            _output.WriteLine($"Item.RaceType: {val.RaceType}");
            _output.WriteLine($"Item.RaceTypeName: {val.RaceTypeName}");
            _output.WriteLine($"Item.DateCreated: {val.DateCreated}");
            _output.WriteLine($"Item.RaceId: {val.RaceId}");

            Assert.Equal("President", val.Name);
            Assert.Equal("ChooseOne", val.RaceType.ToString());
            Assert.IsType<DateTime>(val.DateCreated);
            Assert.NotEmpty(val.RaceId);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesInvalidRaceCreate()
        {
            // This object is missing required property (RaceType)
            var fakeBaseRaceObj = new FakeBaseRaceModel { Name = "President" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(fakeBaseRaceObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _raceApi.CreateRace(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);
            Assert.Contains("Required", objectResult.Value.ToString());

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task FindsRace()
        {
            var findRaceData = MoqData.MockRaceData;
            findRaceData[0].Candidates = _moqDataAccessor.mockCandidateDataCollection;

            var mockRaceSet = DbMoqHelper.GetDbSet(findRaceData.AsQueryable());

            var mockRaceContext = new Mock<TrueVoteDbContext>();
            mockRaceContext.Setup(m => m.Races).Returns(mockRaceSet.Object);

            var findRaceObj = new FindRaceModel { Name = "President" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findRaceObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var raceApi = new Race(_logHelper.Object, mockRaceContext.Object, _mockTelegram.Object);

            var ret = await raceApi.RaceFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<OkObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.OK, objectResult.StatusCode);

            var val = objectResult.Value as List<RaceModel>;
            Assert.NotEmpty(val);
            Assert.Single(val);
            Assert.Equal("President", val[0].Name);
            Assert.Equal("John Smith", val[0].Candidates.ToList()[0].Name);
            Assert.Equal("Jane Doe", val[0].Candidates.ToList()[1].Name);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundRace()
        {
            var findRaceObj = new FindRaceModel { Name = "not going to find anything" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findRaceObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var raceApi = new Race(_logHelper.Object, _moqDataAccessor.mockRaceContext.Object, _mockTelegram.Object);

            var ret = await raceApi.RaceFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<NotFoundResult>(ret);
            Assert.Equal((int) HttpStatusCode.NotFound, objectResult.StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesFindRaceError()
        {
            var findRaceObj = "blah";
            var byteArray = Encoding.ASCII.GetBytes(findRaceObj);
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _raceApi.RaceFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsCandidatesToRace()
        {
            var addsCandidatesRaceData = MoqData.MockRaceData;

            // https://docs.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking?redirectedfrom=MSDN
            // https://github.com/romantitov/MockQueryable
            var mockRaceContext = new Mock<TrueVoteDbContext>();

            var mockRaceSet = addsCandidatesRaceData.AsQueryable().BuildMockDbSet();
            mockRaceContext.Setup(m => m.Races).Returns(mockRaceSet.Object);

            var mockCandidatesSet = MoqData.MockCandidateData.AsQueryable().BuildMockDbSet();
            mockRaceContext.Setup(m => m.Candidates).Returns(mockCandidatesSet.Object);

            var addCandidatesObj = new AddCandidatesModel { RaceId = "1", CandidateIds = new List<string> { MoqData.MockCandidateData[0].CandidateId, MoqData.MockCandidateData[1].CandidateId } };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(addCandidatesObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var raceApi = new Race(_logHelper.Object, mockRaceContext.Object, _mockTelegram.Object);

            var ret = await raceApi.AddCandidates(_httpContext.Request);

            Assert.NotNull(ret);
            var objectResult = Assert.IsType<CreatedResult>(ret);
            Assert.Equal((int) HttpStatusCode.Created, objectResult.StatusCode);

            var val = objectResult.Value as RaceModel;
            Assert.NotNull(val);
            Assert.Equal("President", val.Name);
            Assert.Equal("John Smith", val.Candidates.ToList()[0].Name);
            Assert.Equal("Republican", val.Candidates.ToList()[0].PartyAffiliation);
            Assert.Equal("Jane Doe", val.Candidates.ToList()[1].Name);
            Assert.Equal("Democrat", val.Candidates.ToList()[1].PartyAffiliation);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesAddCandidatesError()
        {
            var addCandidatesObj = "blah";
            var byteArray = Encoding.ASCII.GetBytes(addCandidatesObj);
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _raceApi.AddCandidates(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesAddCandidatesUnfoundRace()
        {
            var addsCandidatesRaceData = MoqData.MockRaceData;

            var mockRaceContext = new Mock<TrueVoteDbContext>();

            var mockRaceSet = addsCandidatesRaceData.AsQueryable().BuildMockDbSet();
            mockRaceContext.Setup(m => m.Races).Returns(mockRaceSet.Object);

            var addCandidatesObj = new AddCandidatesModel { RaceId = "blah", CandidateIds = new List<string>() { } };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(addCandidatesObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var raceApi = new Race(_logHelper.Object, mockRaceContext.Object, _mockTelegram.Object);

            var ret = await raceApi.AddCandidates(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<NotFoundObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.NotFound, objectResult.StatusCode);
            Assert.Contains("Race", objectResult.Value.ToString());
            Assert.Contains("not found", objectResult.Value.ToString());

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(1));
        }

        [Fact]
        public async Task HandlesAddCandidatesUnfoundCandidate()
        {
            var addsCandidatesRaceData = MoqData.MockRaceData;

            var mockRaceContext = new Mock<TrueVoteDbContext>();

            var mockRaceSet = addsCandidatesRaceData.AsQueryable().BuildMockDbSet();
            mockRaceContext.Setup(m => m.Races).Returns(mockRaceSet.Object);

            var mockCandidatesSet = _moqDataAccessor.mockCandidateDataQueryable.AsQueryable().BuildMockDbSet();
            mockRaceContext.Setup(m => m.Candidates).Returns(mockCandidatesSet.Object);

            var addCandidatesObj = new AddCandidatesModel { RaceId = "1", CandidateIds = new List<string> { "68", "69" } };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(addCandidatesObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var raceApi = new Race(_logHelper.Object, mockRaceContext.Object, _mockTelegram.Object);

            var ret = await raceApi.AddCandidates(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<NotFoundObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.NotFound, objectResult.StatusCode);
            Assert.Contains("Candidate", objectResult.Value.ToString());
            Assert.Contains("not found", objectResult.Value.ToString());

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(1));
        }

        [Fact]
        public async Task HandlesAddCandidateAlreadyInRace()
        {
            var addsCandidatesRaceData = MoqData.MockRaceData;
            addsCandidatesRaceData[0].Candidates = _moqDataAccessor.mockCandidateDataCollection;

            var mockRaceContext = new Mock<TrueVoteDbContext>();

            var mockRaceSet = addsCandidatesRaceData.AsQueryable().BuildMockDbSet();
            mockRaceContext.Setup(m => m.Races).Returns(mockRaceSet.Object);

            var mockCandidatesSet = _moqDataAccessor.mockCandidateDataQueryable.AsQueryable().BuildMockDbSet();
            mockRaceContext.Setup(m => m.Candidates).Returns(mockCandidatesSet.Object);

            var addCandidatesObj = new AddCandidatesModel { RaceId = "1", CandidateIds = new List<string> { "1", "2" } };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(addCandidatesObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var raceApi = new Race(_logHelper.Object, mockRaceContext.Object, _mockTelegram.Object);

            var ret = await raceApi.AddCandidates(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<ConflictObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.Conflict, objectResult.StatusCode);
            Assert.Contains("Candidate", objectResult.Value.ToString());
            Assert.Contains("already exists", objectResult.Value.ToString());

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(1));
        }
    }
}

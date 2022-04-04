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

            _log.Verify(LogLevel.Information, Times.Exactly(1));
            _log.Verify(LogLevel.Debug, Times.Exactly(2));
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

            _log.Verify(LogLevel.Information, Times.Exactly(1));
            _log.Verify(LogLevel.Debug, Times.Exactly(2));
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

            _log.Verify(LogLevel.Error, Times.Exactly(1));
            _log.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task FindsRace()
        {
            var findRaceData = new List<RaceModel>
            {
                new RaceModel { Name = "President", DateCreated = DateTime.Now, RaceType = RaceTypes.ChooseOne, Candidates = MoqData.MockCandidateDataCollection },
                new RaceModel { Name = "Judge", DateCreated = DateTime.Now.AddSeconds(1), RaceType = RaceTypes.ChooseMany },
                new RaceModel { Name = "Governor", DateCreated = DateTime.Now.AddSeconds(2), RaceType = RaceTypes.ChooseOne }
            }.AsQueryable();

            var mockRaceSet = DbMoqHelper.GetDbSet(findRaceData);

            var mockRaceContext = new Mock<TrueVoteDbContext>();
            mockRaceContext.Setup(m => m.Races).Returns(mockRaceSet.Object);

            var findRaceObj = new FindRaceModel { Name = "President" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findRaceObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var raceApi = new Race(_log.Object, mockRaceContext.Object);

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

            _log.Verify(LogLevel.Information, Times.Exactly(1));
            _log.Verify(LogLevel.Debug, Times.Exactly(2));
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

            _log.Verify(LogLevel.Error, Times.Exactly(1));
            _log.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsCandidatesToRace()
        {
            var addsCandidatesRaceData = new List<RaceModel>
            {
                new RaceModel { Name = "President", DateCreated = DateTime.Now, RaceType = RaceTypes.ChooseOne },
                new RaceModel { Name = "Judge", DateCreated = DateTime.Now.AddSeconds(1), RaceType = RaceTypes.ChooseMany },
                new RaceModel { Name = "Governor", DateCreated = DateTime.Now.AddSeconds(2), RaceType = RaceTypes.ChooseOne }
            };

            addsCandidatesRaceData[0].RaceId = "1";
            addsCandidatesRaceData[1].RaceId = "2";
            addsCandidatesRaceData[2].RaceId = "3";

            // https://docs.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking?redirectedfrom=MSDN
            // https://github.com/romantitov/MockQueryable
            var mockRaceSet = addsCandidatesRaceData.AsQueryable().BuildMockDbSet();

            var mockRaceContext = new Mock<TrueVoteDbContext>();
            mockRaceContext.Setup(m => m.Races).Returns(mockRaceSet.Object);

            var candidates = new List<CandidateModel> {
                new CandidateModel { Name = "John Smith", DateCreated = DateTime.Now, PartyAffiliation = "Republican", CandidateId = "1" },
                new CandidateModel { Name = "Jane Doe", DateCreated = DateTime.Now, PartyAffiliation = "Democrat", CandidateId = "2" }
            };

            var addCandidatesObj = new AddCandidatesModel { RaceId = "1", CandidateIds = new List<string> { candidates[0].CandidateId, candidates[1].CandidateId } };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(addCandidatesObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var raceApi = new Race(_log.Object, mockRaceContext.Object);

            var ret = await raceApi.AddCandidates(_httpContext.Request);

            // TODO Verify candidates are added

            _log.Verify(LogLevel.Information, Times.Exactly(1));
            _log.Verify(LogLevel.Debug, Times.Exactly(2));
        }
    }
}

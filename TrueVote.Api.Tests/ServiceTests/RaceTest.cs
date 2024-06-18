using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueVote.Api.Models;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;

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
        public List<CandidateModel> Candidates { get; set; } = new List<CandidateModel>();
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
            var validationResults = ValidationHelper.Validate(baseRaceObj);
            Assert.Empty(validationResults);

            _ = await _raceApi.CreateRace(baseRaceObj);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsRace()
        {
            var baseRaceObj = new BaseRaceModel { Name = "President", RaceType = RaceTypes.ChooseOne };
            var validationResults = ValidationHelper.Validate(baseRaceObj);
            Assert.Empty(validationResults);

            var ret = await _raceApi.CreateRace(baseRaceObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status201Created, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (RaceModel) (ret as CreatedAtActionResult).Value;
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
        public async Task FindsRace()
        {
            var findRaceObj = new FindRaceModel { Name = "President" };
            var validationResults = ValidationHelper.Validate(findRaceObj);
            Assert.Empty(validationResults);

            var ret = await _raceApi.RaceFind(findRaceObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (RaceModelList) (ret as OkObjectResult).Value;
            Assert.NotEmpty(val.Races);
            Assert.Single(val.Races);
            Assert.Equal("President", val.Races[0].Name);
            Assert.Equal("John Smith", val.Races[0].Candidates.ToList()[0].Name);
            Assert.Equal("Jane Doe", val.Races[0].Candidates.ToList()[1].Name);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundRace()
        {
            var findRaceObj = new FindRaceModel { Name = "not going to find anything" };
            var validationResults = ValidationHelper.Validate(findRaceObj);
            Assert.Empty(validationResults);

            var ret = await _raceApi.RaceFind(findRaceObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsCandidatesToRace()
        {
            var addCandidatesObj = new AddCandidatesModel { RaceId = "raceid3", CandidateIds = new List<string> { MoqData.MockCandidateData[0].CandidateId, MoqData.MockCandidateData[1].CandidateId } };
            var validationResults = ValidationHelper.Validate(addCandidatesObj);
            Assert.Empty(validationResults);

            var ret = await _raceApi.AddCandidates(addCandidatesObj);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status201Created, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (RaceModel) (ret as CreatedAtActionResult).Value;
            Assert.NotNull(val);
            Assert.Equal("Governor", val.Name);
            Assert.Equal("John Smith", val.Candidates.ToList()[0].Name);
            Assert.Equal("Republican", val.Candidates.ToList()[0].PartyAffiliation);
            Assert.Equal("Jane Doe", val.Candidates.ToList()[1].Name);
            Assert.Equal("Democrat", val.Candidates.ToList()[1].PartyAffiliation);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesAddCandidatesUnfoundRace()
        {
            var addCandidatesObj = new AddCandidatesModel { RaceId = "blah", CandidateIds = new List<string>() { } };
            var validationResults = ValidationHelper.Validate(addCandidatesObj);
            Assert.Empty(validationResults);

            var ret = await _raceApi.AddCandidates(addCandidatesObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as NotFoundObjectResult).Value;
            Assert.Contains("Race", val.Value.ToString());
            Assert.Contains("not found", val.Value.ToString());

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(1));
        }

        [Fact]
        public async Task HandlesAddCandidatesUnfoundCandidate()
        {
            var addCandidatesObj = new AddCandidatesModel { RaceId = "raceid1", CandidateIds = new List<string> { "68", "69" } };
            var validationResults = ValidationHelper.Validate(addCandidatesObj);
            Assert.Empty(validationResults);

            var ret = await _raceApi.AddCandidates(addCandidatesObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as NotFoundObjectResult).Value;
            Assert.Contains("Candidate", val.Value.ToString());
            Assert.Contains("not found", val.Value.ToString());

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(1));
        }

        [Fact]
        public async Task HandlesAddCandidateAlreadyInRace()
        {
            var addCandidatesObj = new AddCandidatesModel { RaceId = "raceid1", CandidateIds = new List<string> { "candidateid1", "candidateid2" } };
            var validationResults = ValidationHelper.Validate(addCandidatesObj);
            Assert.Empty(validationResults);

            var ret = await _raceApi.AddCandidates(addCandidatesObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status409Conflict, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as ConflictObjectResult).Value;
            Assert.Contains("Candidate", val.Value.ToString());
            Assert.Contains("already exists", val.Value.ToString());

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(1));
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueVote.Api.Helpers;
using TrueVote.Api.Models;
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
            var baseElectionObj = new BaseElectionModel { Name = "California State", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Description = "desc", HeaderImageUrl = "url", BaseRaces = [] };
            var validationResults = ValidationHelper.Validate(baseElectionObj);
            Assert.Empty(validationResults);

            await _electionApi.CreateElection(baseElectionObj);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsElection()
        {
            var baseCandidateObj = new BaseCandidateModel { Name = "John Smith", PartyAffiliation = "Republican", DateCreated = UtcNowProviderFactory.GetProvider().UtcNow, CandidateImageUrl = "", Selected = false };
            var baseRaceObj = new BaseRaceModel { Name = "President", RaceType = RaceTypes.ChooseOne, BaseCandidates = [baseCandidateObj], DateCreated = UtcNowProviderFactory.GetProvider().UtcNow, MaxNumberOfChoices = 1, MinNumberOfChoices = 1 };
            var baseElectionObj = new BaseElectionModel { Name = "California State", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Description = "desc", HeaderImageUrl = "url", BaseRaces = [baseRaceObj] };
            var validationResults = ValidationHelper.Validate(baseElectionObj);
            Assert.Empty(validationResults);

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
        public async Task FindsElection()
        {
            var findElectionObj = new FindElectionModel { Name = "County" };
            var validationResults = ValidationHelper.Validate(findElectionObj);
            Assert.Empty(validationResults);

            var ret = await _electionApi.ElectionFind(findElectionObj);
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
        public async Task HandlesUnfoundElection()
        {
            var findElectionObj = new FindElectionModel { Name = "not going to find anything" };
            var validationResults = ValidationHelper.Validate(findElectionObj);
            Assert.Empty(validationResults);

            var ret = await _electionApi.ElectionFind(findElectionObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsRacesToElection()
        {
            var addRacesObj = new AddRacesModel { ElectionId = MoqData.MockElectionData[2].ElectionId, RaceIds = new List<string> { MoqData.MockRaceData[0].RaceId, MoqData.MockRaceData[1].RaceId, MoqData.MockRaceData[2].RaceId } };
            var validationResults = ValidationHelper.Validate(addRacesObj);
            Assert.Empty(validationResults);

            var ret = await _electionApi.AddRaces(addRacesObj);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status201Created, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (ElectionModel) (ret as CreatedAtActionResult).Value;
            Assert.NotNull(val);
            Assert.Equal("Federal", val.Name);
            Assert.Equal("President", val.Races.ToList()[0].Name);
            Assert.Equal("Judge", val.Races.ToList()[1].Name);
            Assert.Equal("Governor", val.Races.ToList()[2].Name);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesAddRacesUnfoundElection()
        {
            var addRacesObj = new AddRacesModel { ElectionId = "blah", RaceIds = new List<string>() { } };
            var validationResults = ValidationHelper.Validate(addRacesObj);
            Assert.Empty(validationResults);

            var ret = await _electionApi.AddRaces(addRacesObj);
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
            var addRacesObj = new AddRacesModel { ElectionId = "electionid1", RaceIds = new List<string> { "68", "69", "70" } };
            var validationResults = ValidationHelper.Validate(addRacesObj);
            Assert.Empty(validationResults);

            var ret = await _electionApi.AddRaces(addRacesObj);
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
            var addRacesObj = new AddRacesModel { ElectionId = "electionid1", RaceIds = new List<string> { "raceid1", "raceid2", "raceid3" } };
            var validationResults = ValidationHelper.Validate(addRacesObj);
            Assert.Empty(validationResults);

            var ret = await _electionApi.AddRaces(addRacesObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status409Conflict, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as ConflictObjectResult).Value;
            Assert.Contains("Race", val.Value.ToString());
            Assert.Contains("already exists", val.Value.ToString());

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(1));
        }

        [Fact]
        public async Task HandlesCreateAccessCodesUserNotFound()
        {
            var accessCodesRequest = new AccessCodesRequest { UserId = MoqData.MockUserData[0].UserId, ElectionId = "123", NumberOfAccessCodes = 5, RequestDescription = "Test Harness" };

            accessCodesRequest.UserId = "blah";
            var validationResults = ValidationHelper.Validate(accessCodesRequest);
            Assert.Empty(validationResults);

            _electionApi.ControllerContext = _authControllerContext;
            var ret = await _electionApi.CreateAccessCodes(accessCodesRequest);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesCreateAccessCodesWithoutAuthorization()
        {
            var accessCodesRequest = new AccessCodesRequest { UserId = MoqData.MockUserData[0].UserId, ElectionId = "blah", NumberOfAccessCodes = 5, RequestDescription = "Test Harness" };
            var validationResults = ValidationHelper.Validate(accessCodesRequest);
            Assert.Empty(validationResults);

            var ret = await _electionApi.CreateAccessCodes(accessCodesRequest);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status401Unauthorized, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesCreateAccessCodesUnfoundElection()
        {
            var accessCodesRequest = new AccessCodesRequest { UserId = MoqData.MockUserData[0].UserId, ElectionId = "123", NumberOfAccessCodes = 5, RequestDescription = "Test Harness" };
            var validationResults = ValidationHelper.Validate(accessCodesRequest);
            Assert.Empty(validationResults);

            _electionApi.ControllerContext = _authControllerContext;
            var ret = await _electionApi.CreateAccessCodes(accessCodesRequest);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as NotFoundObjectResult).Value;
            Assert.Contains("Election", val.Value.ToString());
            Assert.Contains("not found", val.Value.ToString());

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task CreatesAccessCodes()
        {
            var numberOfAccessCodes = 5;
            var accessCodesRequest = new AccessCodesRequest { UserId = MoqData.MockUserData[0].UserId, ElectionId = MoqData.MockElectionData[0].ElectionId, NumberOfAccessCodes = numberOfAccessCodes, RequestDescription = "Test Harness" };
            var validationResults = ValidationHelper.Validate(accessCodesRequest);
            Assert.Empty(validationResults);

            _electionApi.ControllerContext = _authControllerContext;
            var ret = await _electionApi.CreateAccessCodes(accessCodesRequest);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status201Created, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (AccessCodesResponse) (ret as CreatedAtActionResult).Value;
            Assert.NotNull(val);

            Assert.NotEmpty(val.RequestId);
            Assert.NotEmpty(val.ElectionId);
            Assert.True(val.AccessCodes.Count == numberOfAccessCodes);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesCheckAccessCodeWithoutAuthorization()
        {
            var checkCodeRequest = new CheckCodeRequest { UserId = MoqData.MockUserData[0].UserId, AccessCode = "blah" };
            var validationResults = ValidationHelper.Validate(checkCodeRequest);
            Assert.Empty(validationResults);

            var ret = await _electionApi.CheckAccessCode(checkCodeRequest);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status401Unauthorized, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesCheckAccessCodeUserNotFound()
        {
            var checkCodeRequest = new CheckCodeRequest { UserId = MoqData.MockUserData[0].UserId, AccessCode = "blah" };

            checkCodeRequest.UserId = "blah";
            var validationResults = ValidationHelper.Validate(checkCodeRequest);
            Assert.Empty(validationResults);

            _electionApi.ControllerContext = _authControllerContext;
            var ret = await _electionApi.CheckAccessCode(checkCodeRequest);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesCheckAccessCodeUnfoundAccessCode()
        {
            var checkCodeRequest = new CheckCodeRequest { UserId = MoqData.MockUserData[0].UserId, AccessCode = "blah" };
            var validationResults = ValidationHelper.Validate(checkCodeRequest);
            Assert.Empty(validationResults);

            _electionApi.ControllerContext = _authControllerContext;
            var ret = await _electionApi.CheckAccessCode(checkCodeRequest);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as NotFoundObjectResult).Value;
            Assert.Contains("AccessCode", val.Value.ToString());
            Assert.Contains("not found", val.Value.ToString());

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesCheckAccessCodeUnfoundElection()
        {
            var checkCodeRequest = new CheckCodeRequest { UserId = MoqData.MockUserData[0].UserId, AccessCode = "accesscode3" };
            var validationResults = ValidationHelper.Validate(checkCodeRequest);
            Assert.Empty(validationResults);

            _electionApi.ControllerContext = _authControllerContext;
            var ret = await _electionApi.CheckAccessCode(checkCodeRequest);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as NotFoundObjectResult).Value;
            Assert.Contains("Election", val.Value.ToString());
            Assert.Contains("not found", val.Value.ToString());

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task ReturnsElectionFromAccessCode()
        {
            var checkCodeRequest = new CheckCodeRequest { UserId = MoqData.MockUserData[0].UserId, AccessCode = "accesscode1" };
            var validationResults = ValidationHelper.Validate(checkCodeRequest);
            Assert.Empty(validationResults);

            _electionApi.ControllerContext = _authControllerContext;
            var ret = await _electionApi.CheckAccessCode(checkCodeRequest);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (ElectionModel) (ret as OkObjectResult).Value;
            Assert.NotNull(val);
            Assert.Equal("California State", val.Name);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsUsedAccessCode()
        {
            var usedAccessCode = new UsedAccessCodeModel { AccessCode = "accesscode3" };

            var ret = await _electionApi.UseAccessCode(usedAccessCode);
            Assert.True(ret);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueVote.Api.Helpers;
using TrueVote.Api.Models;
using TrueVote.Api.Services;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class BallotTest : TestHelper
    {
        public BallotTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SubmitsBallot()
        {
            var baseBallotObj = new SubmitBallotModel { AccessCode = MoqData.MockElectionAccessCodeData[0].AccessCode, Election = MoqData.MockBallotData[1].Election };

            var user = MoqData.MockUserData[0];
            _ballotApi.SetupController(user.UserId);
            var ret = await _ballotApi.SubmitBallot(baseBallotObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status201Created, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SubmitBallotModelResponse) (ret as CreatedAtActionResult).Value;
            Assert.NotNull(val);

            _output.WriteLine($"Item: {val}");

            _output.WriteLine($"Item.Message: {val.Message}");
            _output.WriteLine($"Item.ElectionId: {val.ElectionId}");

            Assert.Contains("Ballot successfully submitted.", val.Message);
            Assert.NotEmpty(val.ElectionId);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task SubmitsBallotWithAboveCandidateCountNumberOfMaxChoices()
        {
            var baseBallotObj = new SubmitBallotModel { AccessCode = MoqData.MockUsedAccessCodeData[0].AccessCode, Election = MoqData.MockBallotData[1].Election };
            baseBallotObj.Election.Races[0].MaxNumberOfChoices = 1;
            baseBallotObj.Election.Races[0].Candidates[0].Selected = true;
            baseBallotObj.Election.Races[0].Candidates[1].Selected = true;

            var validationErrors = new Dictionary<string, string[]>
            {
                { "cannot exceed MaxNumberOfChoices for", ["MaxNumberOfChoices"] }
            };

            var mockRecursiveValidator = new Mock<IRecursiveValidator>();
            mockRecursiveValidator.Setup(m => m.TryValidateObjectRecursive(It.IsAny<object>(), It.IsAny<ValidationContext>(), It.IsAny<List<ValidationResult>>())).Returns(false);
            mockRecursiveValidator.Setup(m => m.GetValidationErrorsDictionary(It.IsAny<List<ValidationResult>>())).Returns(validationErrors);

            var ballotApi = new Ballot(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, _mockServiceBus.Object, mockRecursiveValidator.Object, _mockTopicEventSender.Object, _queryService);

            var ret = await ballotApi.SubmitBallot(baseBallotObj);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status400BadRequest, ((IStatusCodeActionResult) ret).StatusCode);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            var validationProblemDetails = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
            Assert.Contains("cannot exceed MaxNumberOfChoices", validationProblemDetails.Errors.Keys.First());
            Assert.Contains("MaxNumberOfChoices", validationProblemDetails.Errors.Values.First());
        }

        [Fact]
        public async Task SubmitsBallotWithBelowCandidateCountNumberOfMinChoices()
        {
            var baseBallotObj = new SubmitBallotModel { AccessCode = MoqData.MockUsedAccessCodeData[0].AccessCode, Election = MoqData.MockBallotData[1].Election };
            baseBallotObj.Election.Races[0].MinNumberOfChoices = 2;
            baseBallotObj.Election.Races[0].Candidates[0].Selected = true;

            var validationErrors = new Dictionary<string, string[]>
            {
                { "must be greater or equal to MinNumberOfChoices for", ["MinNumberOfChoices"] }
            };

            var mockRecursiveValidator = new Mock<IRecursiveValidator>();
            mockRecursiveValidator.Setup(m => m.TryValidateObjectRecursive(It.IsAny<object>(), It.IsAny<ValidationContext>(), It.IsAny<List<ValidationResult>>())).Returns(false);
            mockRecursiveValidator.Setup(m => m.GetValidationErrorsDictionary(It.IsAny<List<ValidationResult>>())).Returns(validationErrors);

            var ballotApi = new Ballot(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, _mockServiceBus.Object, mockRecursiveValidator.Object, _mockTopicEventSender.Object, _queryService);

            var ret = await ballotApi.SubmitBallot(baseBallotObj);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status400BadRequest, ((IStatusCodeActionResult) ret).StatusCode);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            var validationProblemDetails = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
            Assert.Contains("must be greater or equal", validationProblemDetails.Errors.Keys.First());
            Assert.Contains("MinNumberOfChoices", validationProblemDetails.Errors.Values.First());
        }

        [Fact]
        public async Task FindsBallot()
        {
            var findBallotObj = new FindBallotModel { BallotId = "ballotid3" };

            var ret = await _ballotApi.BallotFind(findBallotObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (BallotList) (ret as OkObjectResult).Value;
            Assert.NotNull(val);
            Assert.Equal("ballotid3", val.Ballots[0].BallotId);
            Assert.Equal("electionid1", val.Ballots[0].Election.ElectionId);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundBallot()
        {
            var findBallotObj = new FindBallotModel { BallotId = "not going to find anything" };

            var ret = await _ballotApi.BallotFind(findBallotObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task CountsBallots()
        {
            var countBallotsObj = new CountBallotModel { DateCreatedStart = new DateTime(2022, 01, 01), DateCreatedEnd = new DateTime(2033, 12, 31) };

            var ret = await _ballotApi.BallotCount(countBallotsObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (CountBallotModelResponse) (ret as OkObjectResult).Value;
            Assert.NotNull(val);
            Assert.Equal(5, val.BallotCount);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task FindsBallotHash()
        {
            var findBallotHashObj = new FindBallotHashModel { BallotId = "ballotid1" };

            var ret = await _ballotApi.BallotHashFind(findBallotHashObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (List<BallotHashModel>) (ret as OkObjectResult).Value;
            Assert.NotEmpty(val);
            Assert.Single(val);
            Assert.Equal("ballotid1", val[0].BallotId);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundBallotHash()
        {
            var findBallotHashObj = new FindBallotHashModel { BallotId = "not going to find anything" };

            var ret = await _ballotApi.BallotHashFind(findBallotHashObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesSubmitBallotDatabaseError()
        {
            var baseBallotObj = new SubmitBallotModel { AccessCode = MoqData.MockElectionAccessCodeData[0].AccessCode, Election = MoqData.MockBallotData[1].Election };

            var mockBallotContext = new Mock<MoqTrueVoteDbContext>();

            var mockBallotDataQueryable = MoqData.MockBallotData.AsQueryable();
            var MockBallotSet = DbMoqHelper.GetDbSet(mockBallotDataQueryable);
            mockBallotContext.Setup(m => m.Ballots).Returns(MockBallotSet.Object);
            mockBallotContext.Setup(m => m.ElectionAccessCodes).Returns(_moqDataAccessor.MockElectionAccessCodeSet.Object);
            mockBallotContext.Setup(m => m.UsedAccessCodes).Returns(_moqDataAccessor.MockUsedAccessCodeSet.Object);
            mockBallotContext.Setup(m => m.ElectionUserBindings).Returns(_moqDataAccessor.MockElectionUserBindingsSet.Object);
            mockBallotContext.Setup(m => m.SaveChangesAsync()).Throws(new Exception("DB Saving Changes Exception"));

            var ballotApi = new Ballot(_logHelper.Object, mockBallotContext.Object, _mockServiceBus.Object, _mockRecursiveValidator.Object, _mockTopicEventSender.Object, _queryService);
            var user = MoqData.MockUserData[0];
            ballotApi.SetupController(user.UserId);

            var ret = await ballotApi.SubmitBallot(baseBallotObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status409Conflict, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as ConflictObjectResult).Value;
            Assert.NotNull(val);
            Assert.Contains("DB Saving Changes Exception", val.Value);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesSubmitBallotUnfoundAccessCode()
        {
            var baseBallotObj = new SubmitBallotModel { AccessCode = "blah", Election = MoqData.MockBallotData[1].Election };

            var user = MoqData.MockUserData[0];
            _ballotApi.SetupController(user.UserId);
            var ret = await _ballotApi.SubmitBallot(baseBallotObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as NotFoundObjectResult).Value;
            Assert.NotNull(val);
            Assert.Contains("AccessCode", val.Value);
            Assert.Contains("not found", val.Value);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesSubmitBallotAlreadyUsedAccessCode()
        {
            var user = MoqData.MockUserData[0];
            _ballotApi.SetupController(user.UserId);
            var baseBallotObj = new SubmitBallotModel { AccessCode = MoqData.MockUsedAccessCodeData[0].AccessCode, Election = MoqData.MockBallotData[1].Election };

            var ret = await _ballotApi.SubmitBallot(baseBallotObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status409Conflict, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as ConflictObjectResult).Value;
            Assert.NotNull(val);
            Assert.Contains("AccessCode", val.Value);
            Assert.Contains("already used", val.Value);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesSubmitBallotUserAlreadySubmitted()
        {
            var baseBallotObj = new SubmitBallotModel { AccessCode = MoqData.MockUsedAccessCodeData[0].AccessCode, Election = MoqData.MockBallotData[1].Election };

            var user = MoqData.MockUserData[2];
            _ballotApi.SetupController(user.UserId);
            var ret = await _ballotApi.SubmitBallot(baseBallotObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status409Conflict, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as ConflictObjectResult).Value;
            Assert.NotNull(val);
            Assert.Contains("Ballot already submitted", val.Value);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task GetsBallotsWithoutHashes()
        {
            var ret = await _ballotApi.GetBallotsWithoutHashesAsync(new CancellationToken());
            Assert.NotNull(ret);
            Assert.True(ret.Count > 0);

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(1));
        }

        [Fact]
        public async Task HandlesGetsBallotsWithoutHashesException()
        {
            var baseBallotObj = new SubmitBallotModel { AccessCode = MoqData.MockElectionAccessCodeData[0].AccessCode, Election = MoqData.MockBallotData[1].Election };

            var mockBallotContext = new Mock<MoqTrueVoteDbContext>();

            var mockBallotDataQueryable = MoqData.MockBallotData.AsQueryable();
            var MockBallotSet = DbMoqHelper.GetDbSet(mockBallotDataQueryable);
            mockBallotContext.Setup(m => m.Ballots).Throws(new Exception("Ballots are null")); ;

            var ballotApi = new Ballot(_logHelper.Object, mockBallotContext.Object, _mockServiceBus.Object, _mockRecursiveValidator.Object, _mockTopicEventSender.Object, _queryService);
            var ret = await ballotApi.GetBallotsWithoutHashesAsync(new CancellationToken());
            Assert.Null(ret);
            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
        }
    }
}

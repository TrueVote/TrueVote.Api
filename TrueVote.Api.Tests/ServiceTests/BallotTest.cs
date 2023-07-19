using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
    public class BallotTest : TestHelper
    {
        public BallotTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SubmitsBallot()
        {
            var electionObj = new ElectionModel { ElectionId = "68", Name = "California State", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) };
            var baseBallotObj = new SubmitBallotModel { ElectionId = "68", Election = electionObj };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(baseBallotObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _ballotApi.SubmitBallot(_httpContext.Request) as CreatedResult;
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<CreatedResult>(ret);
            Assert.Equal((int) HttpStatusCode.Created, objectResult.StatusCode);

            var val = ret.Value as SubmitBallotModelResponse;
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
        public async Task HandlesSubmitBallotError()
        {
            var baseBallotObj = "blah";
            var byteArray = Encoding.ASCII.GetBytes(baseBallotObj);
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _ballotApi.SubmitBallot(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesFindBallotError()
        {
            var findBallotObj = "blah";
            var byteArray = Encoding.ASCII.GetBytes(findBallotObj);
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _ballotApi.BallotFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task FindsBallot()
        {
            var findBallotObj = new FindBallotModel { BallotId = "ballotid3" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findBallotObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ballotApi = new Ballot(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, _mockTelegram.Object, _validatorApi);

            var ret = await ballotApi.BallotFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<OkObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.OK, objectResult.StatusCode);

            var val = objectResult.Value as List<BallotModel>;
            Assert.NotEmpty(val);
            Assert.Single(val);
            Assert.Equal("ballotid3", val[0].BallotId);
            Assert.Equal("electionid1", val[0].ElectionId);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundBallot()
        {
            var findBallotObj = new FindBallotModel { BallotId = "not going to find anything" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findBallotObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ballotApi = new Ballot(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, _mockTelegram.Object, _validatorApi);

            var ret = await ballotApi.BallotFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<NotFoundResult>(ret);
            Assert.Equal((int) HttpStatusCode.NotFound, objectResult.StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task CountsBallots()
        {
            var countBallotsObj = new CountBallotModel { DateCreatedStart = new DateTime(2022, 01, 01), DateCreatedEnd = new DateTime(2033, 12, 31) };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(countBallotsObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ballotApi = new Ballot(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, _mockTelegram.Object, _validatorApi);

            var ret = await ballotApi.BallotCount(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<OkObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.OK, objectResult.StatusCode);

            var val = objectResult.Value;
            Assert.Equal(3, val);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesCountBallotsError()
        {
            var countBallotsObj = "blah";
            var byteArray = Encoding.ASCII.GetBytes(countBallotsObj);
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _ballotApi.BallotCount(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task FindsBallotHash()
        {
            var findBallotHashObj = new FindBallotHashModel { BallotId = "ballotid1" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findBallotHashObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _ballotApi.BallotHashFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<OkObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.OK, objectResult.StatusCode);

            var val = objectResult.Value as List<BallotHashModel>;
            Assert.NotEmpty(val);
            Assert.Single(val);
            Assert.Equal("ballotid1", val[0].BallotId);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }
        [Fact]
        public async Task HandlesFindBallotHashError()
        {
            var findBallotHashObj = "blah";
            var byteArray = Encoding.ASCII.GetBytes(findBallotHashObj);
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _ballotApi.BallotHashFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundBallotHash()
        {
            var findBallotHashObj = new FindBallotHashModel { BallotId = "not going to find anything" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findBallotHashObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _ballotApi.BallotHashFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<NotFoundResult>(ret);
            Assert.Equal((int) HttpStatusCode.NotFound, objectResult.StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }
    }
}

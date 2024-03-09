/*
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
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
            var baseBallotObj = new SubmitBallotModel { Election = MoqData.MockBallotData[1].Election };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(baseBallotObj));

            var ret = await _ballotApi.SubmitBallot(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.Created, ret.StatusCode);

            var val = await ret.ReadAsJsonAsync<SubmitBallotModelResponse>();
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
            var requestData = new MockHttpRequestData(baseBallotObj);

            var ret = await _ballotApi.SubmitBallot(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.BadRequest, ret.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesSubmitBallotHashingError()
        {
            var baseBallotObj = new SubmitBallotModel { Election = MoqData.MockBallotData[1].Election };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(baseBallotObj));

            var mockValidator = new Mock<IValidator>();
            mockValidator.Setup(m => m.HashBallotAsync(It.IsAny<BallotModel>())).Throws(new Exception("Hash Ballot Exception"));

            var ballotApi = new Ballot(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, mockValidator.Object, _mockServiceBus.Object);
            var ret = await ballotApi.SubmitBallot(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.Conflict, ret.StatusCode);

            var val = await ret.ReadAsJsonAsync<SecureString>();
            Assert.NotNull(val);

            Assert.Contains("Hash Ballot Exception", val.Value);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesFindBallotError()
        {
            var findBallotObj = "blah";
            var requestData = new MockHttpRequestData(findBallotObj);

            var ret = await _ballotApi.BallotFind(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.BadRequest, ret.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task FindsBallot()
        {
            var findBallotObj = new FindBallotModel { BallotId = "ballotid3" };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(findBallotObj));

            var ballotApi = new Ballot(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, _validatorApi, _mockServiceBus.Object);

            var ret = await ballotApi.BallotFind(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.OK, ret.StatusCode);

            var val = await ret.ReadAsJsonAsync<BallotList>();
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
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(findBallotObj));

            var ballotApi = new Ballot(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, _validatorApi, _mockServiceBus.Object);

            var ret = await ballotApi.BallotFind(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.NotFound, ret.StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task CountsBallots()
        {
            var countBallotsObj = new CountBallotModel { DateCreatedStart = new DateTime(2022, 01, 01), DateCreatedEnd = new DateTime(2033, 12, 31) };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(countBallotsObj));

            var ballotApi = new Ballot(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, _validatorApi, _mockServiceBus.Object);

            var ret = await ballotApi.BallotCount(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.OK, ret.StatusCode);
            var val = await ret.ReadAsJsonAsync<CountBallotModelResponse>();
            Assert.NotNull(val);

            Assert.Equal(3, val.BallotCount);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesCountBallotsError()
        {
            var countBallotsObj = "blah";
            var requestData = new MockHttpRequestData(countBallotsObj);

            var ret = await _ballotApi.BallotCount(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.BadRequest, ret.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task FindsBallotHash()
        {
            var findBallotHashObj = new FindBallotHashModel { BallotId = "ballotid1" };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(findBallotHashObj));

            var ret = await _ballotApi.BallotHashFind(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.OK, ret.StatusCode);
            var val = await ret.ReadAsJsonAsync<List<BallotHashModel>>();

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
            var requestData = new MockHttpRequestData(findBallotHashObj);

            var ret = await _ballotApi.BallotHashFind(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.BadRequest, ret.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundBallotHash()
        {
            var findBallotHashObj = new FindBallotHashModel { BallotId = "not going to find anything" };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(findBallotHashObj));

            var ret = await _ballotApi.BallotHashFind(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.NotFound, ret.StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }
    }
}
*/

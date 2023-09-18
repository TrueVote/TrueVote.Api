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
    public class FakeBaseCandidateModel
    {
        public string Name { get; set; }
        public string PartyAffiliation { get; set; }
    }

    public class CandidateTest : TestHelper
    {
        public CandidateTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task LogsMessages()
        {
            var baseCandidateObj = new BaseCandidateModel { Name = "John Smith", PartyAffiliation = "Republican" };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(baseCandidateObj));

            _ = await _candidateApi.CreateCandidate(requestData);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsCandidate()
        {
            var baseCandidateObj = new BaseCandidateModel { Name = "John Smith", PartyAffiliation = "Republican" };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(baseCandidateObj));

            var ret = await _candidateApi.CreateCandidate(requestData);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<CreatedResult>(ret);
            Assert.Equal((int) HttpStatusCode.Created, objectResult.StatusCode);

            var val = await ret.ReadAsJsonAsync<CandidateModel>();
            Assert.NotNull(val);

            _output.WriteLine($"Item: {val}");

            _output.WriteLine($"Item.Name: {val.Name}");
            _output.WriteLine($"Item.PartyAffiliation: {val.PartyAffiliation}");
            _output.WriteLine($"Item.DateCreated: {val.DateCreated}");
            _output.WriteLine($"Item.CandidateId: {val.CandidateId}");

            Assert.Equal("John Smith", val.Name);
            Assert.Equal("Republican", val.PartyAffiliation);
            Assert.IsType<DateTime>(val.DateCreated);
            Assert.NotEmpty(val.CandidateId);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesInvalidCandidateCreate()
        {
            // This object is missing required property (Name)
            var fakeBaseCandidateObj = new FakeBaseCandidateModel { PartyAffiliation = "Republican" };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(fakeBaseCandidateObj));

            var ret = await _candidateApi.CreateCandidate(requestData);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);
            Assert.Contains("Required", objectResult.Value.ToString());

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task FindsCandidate()
        {
            var findCandidateObj = new FindCandidateModel { Name = "J" };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(findCandidateObj));

            var candidateApi = new Candidate(_logHelper.Object, _moqDataAccessor.mockCandidateContext.Object, _mockTelegram.Object);

            var ret = await candidateApi.CandidateFind(requestData);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<OkObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.OK, objectResult.StatusCode);

            var val = objectResult.Value as List<CandidateModel>;
            Assert.NotEmpty(val);
            Assert.Equal(2, val.Count);
            Assert.Equal("John Smith", val[1].Name);
            Assert.Equal("Democrat", val[0].PartyAffiliation);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundCandidate()
        {
            var findCandidateObj = new FindCandidateModel { Name = "not going to find anything" };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(findCandidateObj));

            var candidateApi = new Candidate(_logHelper.Object, _moqDataAccessor.mockCandidateContext.Object, _mockTelegram.Object);

            var ret = await candidateApi.CandidateFind(requestData);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<NotFoundResult>(ret);
            Assert.Equal((int) HttpStatusCode.NotFound, objectResult.StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesFindCandidateError()
        {
            var findCandidateObj = "blah";
            var requestData = new MockHttpRequestData(findCandidateObj);

            var ret = await _candidateApi.CandidateFind(requestData);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }
    }
}

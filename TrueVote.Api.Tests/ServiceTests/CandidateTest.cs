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

namespace TrueVote.Api.Tests.ServiceTests
{
    public class FakeBaseCandidateModel
    {
        public string Name { get; set; } = string.Empty;
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
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(baseCandidateObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            _ = await _candidateApi.CreateCandidate(_httpContext.Request);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsCandidate()
        {
            var baseCandidateObj = new BaseCandidateModel { Name = "John Smith", PartyAffiliation = "Republican" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(baseCandidateObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _candidateApi.CreateCandidate(_httpContext.Request) as CreatedResult;
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<CreatedResult>(ret);
            Assert.Equal((int) HttpStatusCode.Created, objectResult.StatusCode);

            var val = ret.Value as CandidateModel;
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
            // This object is missing required property (PartyAffiliation)
            var fakeBaseCandidateObj = new FakeBaseCandidateModel { Name = "John Smith" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(fakeBaseCandidateObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _candidateApi.CreateCandidate(_httpContext.Request);
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
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findCandidateObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var candidateApi = new Candidate(_logHelper.Object, _moqDataAccessor.mockCandidateContext.Object, _mockTelegram.Object);

            var ret = await candidateApi.CandidateFind(_httpContext.Request);
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
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findCandidateObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var candidateApi = new Candidate(_logHelper.Object, _moqDataAccessor.mockCandidateContext.Object, _mockTelegram.Object);

            var ret = await candidateApi.CandidateFind(_httpContext.Request);
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
            var byteArray = Encoding.ASCII.GetBytes(findCandidateObj);
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _candidateApi.CandidateFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }
    }
}

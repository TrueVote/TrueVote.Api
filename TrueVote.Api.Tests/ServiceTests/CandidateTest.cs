using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using TrueVote.Api.Models;
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

            await _candidateApi.CreateCandidate(baseCandidateObj);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsCandidate()
        {
            var baseCandidateObj = new BaseCandidateModel { Name = "John Smith", PartyAffiliation = "Republican" };

            var ret = await _candidateApi.CreateCandidate(baseCandidateObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status201Created, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (CandidateModel) (ret as CreatedAtActionResult).Value;
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
        public async Task FindsCandidate()
        {
            var findCandidateObj = new FindCandidateModel { Name = "J" };

            var ret = await _candidateApi.CandidateFind(findCandidateObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (CandidateModelList) (ret as OkObjectResult).Value;
            Assert.NotEmpty(val.Candidates);
            Assert.Equal(2, val.Candidates.Count);
            Assert.Equal("John Smith", val.Candidates[1].Name);
            Assert.Equal("Democrat", val.Candidates[0].PartyAffiliation);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundCandidate()
        {
            var findCandidateObj = new FindCandidateModel { Name = "not going to find anything" };

            var ret = await _candidateApi.CandidateFind(findCandidateObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }
    }
}

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
            var baseElectionObj = new BaseElectionModel { Name = "California State", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(baseElectionObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            _ = await _electionApi.CreateElection(_httpContext.Request);

            _log.Verify(LogLevel.Information, Times.Exactly(1));
            _log.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsElection()
        {
            var baseElectionObj = new BaseElectionModel { Name = "California State", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(baseElectionObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _electionApi.CreateElection(_httpContext.Request) as CreatedResult;
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<CreatedResult>(ret);
            Assert.Equal((int) HttpStatusCode.Created, objectResult.StatusCode);

            var val = ret.Value as ElectionModel;
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

            _log.Verify(LogLevel.Information, Times.Exactly(1));
            _log.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesInvalidElectionCreate()
        {
            // This object is missing required property (StartDate)
            var fakeBaseElectionObj = new FakeBaseElectionModel { Name = "California State" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(fakeBaseElectionObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _electionApi.CreateElection(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);
            Assert.Contains("Required", objectResult.Value.ToString());

            _log.Verify(LogLevel.Error, Times.Exactly(1));
            _log.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task FindsElection()
        {
            var findElectionData = new List<ElectionModel>
            {
                new ElectionModel { Name = "California State", DateCreated = DateTime.Now, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) },
                new ElectionModel { Name = "Los Angeles County", DateCreated = DateTime.Now, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) },
                new ElectionModel { Name = "Federal", DateCreated = DateTime.Now, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) },
            }.AsQueryable();
            var mockElectionSet = DbMoqHelper.GetDbSet(findElectionData);

            var mockElectionContext = new Mock<TrueVoteDbContext>();
            mockElectionContext.Setup(m => m.Elections).Returns(mockElectionSet.Object);

            var findElectionObj = new FindElectionModel { Name = "County" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findElectionObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var electionApi = new Election(_log.Object, mockElectionContext.Object);

            var ret = await electionApi.ElectionFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<OkObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.OK, objectResult.StatusCode);

            var val = objectResult.Value as List<ElectionModel>;
            Assert.NotEmpty(val);
            Assert.Single(val);
            Assert.Equal("Los Angeles County", val[0].Name);

            _log.Verify(LogLevel.Information, Times.Exactly(1));
            _log.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesFindElectionError()
        {
            var findElectionObj = "blah";
            var byteArray = Encoding.ASCII.GetBytes(findElectionObj);
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _electionApi.ElectionFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);

            _log.Verify(LogLevel.Error, Times.Exactly(1));
            _log.Verify(LogLevel.Debug, Times.Exactly(2));
        }
    }
}

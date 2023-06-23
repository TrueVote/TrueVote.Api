using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using TrueVote.Api.Models;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.Services
{
    public class TimestampTest : TestHelper
    {
        public TimestampTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FindsTimestamp()
        {
            var findTimestampObj = new FindTimestampModel { DateCreatedStart = new DateTime(2022, 01, 01), DateCreatedEnd = new DateTime(2024, 01, 01) };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findTimestampObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _timestampApi.TimestampFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<OkObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.OK, objectResult.StatusCode);

            var val = objectResult.Value as List<TimestampModel>;
            Assert.NotEmpty(val);
            Assert.Equal(2, val.Count);
            Assert.Equal("2", val[0].TimestampId);
            Assert.Equal("SampleHash2", val[0].TimestampHashS);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesFindTimestampError()
        {
            var findTimestampObj = "blah";
            var byteArray = Encoding.ASCII.GetBytes(findTimestampObj);
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _timestampApi.TimestampFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundTimestamp()
        {
            var findTimestampObj = new FindTimestampModel { DateCreatedStart = new DateTime(2021, 01, 01), DateCreatedEnd = new DateTime(2022, 01, 01) };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findTimestampObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _timestampApi.TimestampFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<NotFoundResult>(ret);
            Assert.Equal((int) HttpStatusCode.NotFound, objectResult.StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }
    }
}

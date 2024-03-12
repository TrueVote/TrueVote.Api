using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
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

            var ret = await _timestampApi.TimestampFind(findTimestampObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (List<TimestampModel>) (ret as OkObjectResult).Value;
            Assert.NotEmpty(val);

            Assert.Equal(2, val.Count);
            Assert.Equal("2", val[0].TimestampId);
            Assert.Equal("SampleHash2", val[0].TimestampHashS);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundTimestamp()
        {
            var findTimestampObj = new FindTimestampModel { DateCreatedStart = new DateTime(2021, 01, 01), DateCreatedEnd = new DateTime(2022, 01, 01) };

            var ret = await _timestampApi.TimestampFind(findTimestampObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }
    }
}

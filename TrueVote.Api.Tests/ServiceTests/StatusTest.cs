using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using TrueVote.Api.Models;
using TrueVote.Api.Services;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class StatusTest : TestHelper
    {
        public StatusTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FailsIfNullArgs()
        {
            try
            {
                var status = new Status(_logHelper.Object);
                _ = await status.GetStatus(null);
            }
            catch (ArgumentNullException ane)
            {
                _output.WriteLine($"{ane}");
                Assert.NotNull(ane);
                Assert.Contains("Value cannot be null", ane.Message);
            }
            catch (NullReferenceException nre)
            {
                _output.WriteLine($"{nre}");
                Assert.NotNull(nre);
                Assert.Contains("Object reference not set to an instance of an object", nre.Message);
            }
        }

        [Fact]
        public async Task LogsMessages()
        {
            var status = new Status(_logHelper.Object);
            var requestData = new MockHttpRequestData("");
            _ = await status.GetStatus(requestData);

            _logHelper.Verify(LogLevel.Information, Times.AtLeast(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task ReturnsValidModel()
        {
            var status = new Status(_logHelper.Object);
            var requestData = new MockHttpRequestData("");
            var res = await status.GetStatus(requestData);

            Assert.NotNull(res);
            var statusModel = await res.ReadAsJsonAsync<StatusModel>();
            Assert.NotNull(statusModel);
            Assert.Equal("TrueVote.Api is responding", statusModel.RespondsMsg);
        }

        [Fact]
        public async Task ReturnsValidBuildInfoModel()
        {
            var status = new Status(_logHelper.Object);
            var requestData = new MockHttpRequestData("");
            var res = await status.GetStatus(requestData);

            Assert.NotNull(res);
            var statusModel = await res.ReadAsJsonAsync<StatusModel>();
            Assert.NotNull(statusModel);
            Assert.NotNull(statusModel.BuildInfo);
        }

        [Fact]
        public async Task RunsStopwatch()
        {
            var status = new Status(_logHelper.Object);
            var requestData = new MockHttpRequestData("");
            var res = await status.GetStatus(requestData);

            Assert.NotNull(res);
            var statusModel = await res.ReadAsJsonAsync<StatusModel>();
            Assert.NotNull(statusModel);
            Assert.True(statusModel.ExecutionTime >= 0);
        }
    }
}

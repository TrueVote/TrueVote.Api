using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using TrueVote.Api.Services;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class Error500Test : TestHelper
    {
        public Error500Test(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task LogsMessages()
        {
            var error500 = new Error500(_logHelper.Object, _mockServiceBus.Object);
            var requestData = new MockHttpRequestData("");
            _ = await error500.ThrowError500(requestData);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task CausesDivideByZero()
        {
            var error500 = new Error500(_logHelper.Object, _mockServiceBus.Object);

            var errorObj = new
            {
                Error = true
            };

            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(errorObj));
            
            try
            {
                _ = await error500.ThrowError500(requestData);
                Assert.True(false);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"{ex}");
                Assert.NotNull(ex);
                Assert.Contains("error500 - throwing a sample exception", ex.Message);
            }
        }
    }
}

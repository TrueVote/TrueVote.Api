using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
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
            var error500 = new Error500(_log.Object);
            _ = await error500.ThrowError500(_httpContext.Request);

            _log.Verify(LogLevel.Information, Times.AtLeast(1));
            _log.Verify(LogLevel.Debug, Times.AtLeast(2));
        }

        [Fact]
        public async Task CausesDivideByZero()
        {
            var error500 = new Error500(_log.Object);

            var errorObj = new
            {
                Error = true
            };

            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(errorObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);
            try
            {
                _ = await error500.ThrowError500(_httpContext.Request);
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

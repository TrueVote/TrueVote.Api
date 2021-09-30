using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using System.IO;
using System.Text;
using System;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class Error500
    {
        private readonly ITestOutputHelper _output;
        private readonly HttpContext _httpContext;
        private readonly Mock<ILogger<Api.Error500>> _log;

        public Error500(ITestOutputHelper output)
        {
            _output = output;
            _httpContext = new DefaultHttpContext();
            _log = new Mock<ILogger<Api.Error500>>();
            _log.MockLog(LogLevel.Debug);
            _log.MockLog(LogLevel.Information);
            _log.MockLog(LogLevel.Warning);
            _log.MockLog(LogLevel.Error);
        }

        [Fact]
        public async Task LogsMessages()
        {
            var error500 = new Api.Error500(_log.Object);
            _ = await error500.RunAsync(_httpContext.Request);

            _log.Verify(LogLevel.Information, Times.AtLeast(1));
            _log.Verify(LogLevel.Debug, Times.AtLeast(2));
        }

        [Fact]
        public async Task CausesDivideByZero()
        {
            var error500 = new Api.Error500(_log.Object);

            var errorObj = new
            {
                Error = true
            };

            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(errorObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);
            try
            {
                _ = await error500.RunAsync(_httpContext.Request);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"{ex}");
                Assert.NotNull(ex);
                Assert.Contains("error500 - throwing an exception", ex.Message);
            }
        }
    }
}

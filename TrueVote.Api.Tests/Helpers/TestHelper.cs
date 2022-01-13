using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions;
using TrueVote.Api.Helpers;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.Helpers
{
    public class TestHelper
    {
        protected readonly ITestOutputHelper _output;
        protected readonly HttpContext _httpContext;
        protected readonly IFileSystem _fileSystem;
        protected readonly Mock<ILogger<LoggerHelper>> _log;

        public TestHelper(ITestOutputHelper output)
        {
            _output = output;
            _httpContext = new DefaultHttpContext();
            _fileSystem = new FileSystem();
            _log = new Mock<ILogger<LoggerHelper>>();
            _log.MockLog(LogLevel.Debug);
            _log.MockLog(LogLevel.Information);
            _log.MockLog(LogLevel.Warning);
            _log.MockLog(LogLevel.Error);
        }
    }
}

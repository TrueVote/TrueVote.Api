using Microsoft.Extensions.Logging;
using Moq;
using TrueVote.Api.Services;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Azure.Functions.Worker;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class HealthTest : TestHelper
    {
        public HealthTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void LogsMessages()
        {
            var health = new Health(_logHelper.Object, _mockTelegram.Object);

            var timerInfo = new TimerInfo();

            health.Run(timerInfo);

            _logHelper.Verify(LogLevel.Information, Times.AtLeast(1));
        }
    }
}

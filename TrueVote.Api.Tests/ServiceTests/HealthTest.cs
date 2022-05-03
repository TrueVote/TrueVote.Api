using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging;
using Moq;
using NCrontab;
using TrueVote.Api.Services;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

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
            var health = new Health(_log.Object);

            var cronSchedule = new CronSchedule(CrontabSchedule.Parse("*/5 * * * *"));

            var status = new ScheduleStatus();
            var timerInfo = new TimerInfo(cronSchedule, status);

            health.Run(timerInfo);

            _log.Verify(LogLevel.Information, Times.AtLeast(1));
        }
    }
}

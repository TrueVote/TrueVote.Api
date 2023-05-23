using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging;
using Moq;
using NCrontab;
using System.Text;
using TrueVote.Api.Services;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class ValidatorTest : TestHelper
    {
        public ValidatorTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CallsValidator()
        {
            var cronSchedule = new CronSchedule(CrontabSchedule.Parse("*/5 * * * *"));

            var status = new ScheduleStatus();
            var timerInfo = new TimerInfo(cronSchedule, status);

            _validatorApi.Run(timerInfo);

            _logHelper.Verify(LogLevel.Information, Times.AtLeast(1));
        }

        [Fact]
        public void HashesBallotData()
        {
            var timestamp = _validatorApi.HashBallots();

            Assert.NotNull(timestamp);
            // Assert.Equal(66, timestamp.MerkleRoot[0]);
        }
    }
}

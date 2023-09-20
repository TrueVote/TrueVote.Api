using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using System;
using TrueVote.Api.Helpers;

namespace TrueVote.Api.Services
{
    public class Health : LoggerHelper
    {
        public Health(ILogger log): base(log)
        {
        }

        [Function("HealthTimer")]
        public void Run([TimerTrigger("*/5 * * * *")] TimerInfo timerInfo)
        {
            LogInformation($"HealthTimer trigger function {timerInfo.ScheduleStatus} executed at: {DateTime.Now.ToUniversalTime():dddd, MMM dd, yyyy HH:mm:ss}");
        }
    }
}

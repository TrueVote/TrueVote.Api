using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using TrueVote.Api.Helpers;

namespace TrueVote.Api.Services
{
    public class Health : LoggerHelper
    {
        public Health(ILogger log, TelegramBot telegramBot): base(log, telegramBot)
        {
        }

        [FunctionName("HealthTimer")]
        public void Run([TimerTrigger("*/5 * * * *")] TimerInfo timerInfo)
        {
            LogInformation($"HealthTimer trigger function {timerInfo.Schedule} executed at: {DateTime.Now.ToUniversalTime():dddd, MMM dd, yyyy HH:mm:ss}");
        }
    }
}

using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using TrueVote.Api.Services;

namespace TrueVote.Api.Helpers
{
    [ExcludeFromCodeCoverage]
    public class LoggerHelper
    {
        private readonly ILogger _log;
        private readonly TelegramBot _telegramBot;

        public LoggerHelper(ILogger log, TelegramBot telegramBot)
        {
            _log = log;
            _telegramBot = telegramBot;
        }

        public void LogInformation(string message)
        {
            _log.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            _log.LogWarning(message);
        }

        public async void LogError(string message)
        {
            await _telegramBot.SendChannelMessageAsync($"TrueVote API Error: {message}");

            _log.LogError(message);
        }

        public void LogCritical(string message)
        {
            _log.LogCritical(message);
        }

        public void LogDebug(string message)
        {
            _log.LogDebug(message);
        }

        public void LogTrace(string message)
        {
            _log.LogTrace(message);
        }
    }
}

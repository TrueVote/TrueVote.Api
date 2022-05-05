using Microsoft.Extensions.Logging;
using TrueVote.Api.Services;

namespace TrueVote.Api.Helpers
{
    public class LoggerHelper
    {
        private ILogger _log;

        public LoggerHelper(ILogger log)
        {
            _log = log;
        }

        public void LogInformation(string message)
        {
            LogInformation(message);
        }

        public void LogWarning(string message)
        {
            LogWarning(message);
        }

        public void LogError(string message)
        {
            _ = TelegramBot.SendChannelMessage($"TrueVote API Error: {message}");

            LogError(message);
        }

        public void LogCritical(string message)
        {
            LogCritical(message);
        }

        public void LogDebug(string message)
        {
            LogDebug(message);
        }

        public void LogTrace(string message)
        {
            LogTrace(message);
        }
    }
}

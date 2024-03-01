using System.Diagnostics.CodeAnalysis;
using TrueVote.Api2.Services;

namespace TrueVote.Api2.Helpers
{
    [ExcludeFromCodeCoverage]
    public class LoggerHelper(ILogger log, IServiceBus serviceBus)
    {
        private readonly ILogger _log = log;
        private readonly IServiceBus _serviceBus = serviceBus;

        public void LogInformation(string message)
        {
            _log.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            _log.LogWarning(message);
        }

        public void LogError(string message)
        {
            _serviceBus.SendAsync($"TrueVote API Error: {message}");

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

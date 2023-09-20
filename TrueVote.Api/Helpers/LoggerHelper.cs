using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Helpers
{
    [ExcludeFromCodeCoverage]
    public class LoggerHelper
    {
        private readonly ILogger _log;

        public LoggerHelper(ILogger log)
        {
            _log = log;
        }

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

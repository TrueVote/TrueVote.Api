using Microsoft.Extensions.Logging;

namespace TrueVote.Api.Helpers
{
    public class LoggerHelper
    {
        protected ILogger _log;

        public LoggerHelper(ILogger log)
        {
            _log = log;
        }
    }
}

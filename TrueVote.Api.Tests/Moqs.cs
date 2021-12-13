using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using System;
using System.Linq.Expressions;

namespace TrueVote.Api.Tests
{
    public static class LoggerMoqHelper
    {
        public static ISetup<ILogger<T>> MockLog<T>(this Mock<ILogger<T>> logger, LogLevel level)
        {
            return logger.Setup(x => x.Log(level, It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
        }

        private static Expression<Action<ILogger<T>>> Verify<T>(LogLevel level)
        {
            return x => x.Log(level, 0, It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>());
        }

        public static void Verify<T>(this Mock<ILogger<T>> mock, LogLevel level, Times times)
        {
            mock.Verify(Verify<T>(level), times);
        }
    }
}

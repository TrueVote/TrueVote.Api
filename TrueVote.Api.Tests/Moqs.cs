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
            return logger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
        }

        private static Expression<Action<ILogger<T>>> Verify<T>(LogLevel level)
        {
            return x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true));
        }

        public static void Verify<T>(this Mock<ILogger<T>> mock, LogLevel level, Times times)
        {
            mock.Verify(Verify<T>(level), times);
        }
    }
}

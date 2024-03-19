using System.Diagnostics.CodeAnalysis;
using TrueVote.Api.Services;

namespace TrueVote.Api.Helpers
{
    [ExcludeFromCodeCoverage]
    public class LoggerHelper : ILogger
    {
        private readonly string _categoryName;
        private readonly IServiceBus _serviceBus;
        private readonly ILogger _log;
        private static readonly object _lock = new object();

        public LoggerHelper(string categoryName, IServiceBus serviceBus, ILogger logger)
        {
            _categoryName = categoryName;
            _serviceBus = serviceBus;
            _log = logger;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _log.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _log.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            lock (_lock)
            {
                var message = $"{logLevel}: [{eventId}] {formatter(state, exception)}";
                _log.Log(logLevel, eventId, state, exception, formatter);
                if (logLevel >= LogLevel.Error)
                {
                    _serviceBus.SendAsync(message).Wait();
                }
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public class CustomLoggerProvider : ILoggerProvider
    {
        private readonly IServiceBus _serviceBus;
        private readonly ILoggerProvider _provider;

        public CustomLoggerProvider(ILoggingBuilder builder)
        {
            _serviceBus = builder.Services.BuildServiceProvider().GetRequiredService<IServiceBus>();
            _provider = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerProvider>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            var logger = _provider.CreateLogger(categoryName);
            return new LoggerHelper(categoryName, _serviceBus, logger);
        }

        public void Dispose()
        {
            _provider?.Dispose();
        }
    }
}

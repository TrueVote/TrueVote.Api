using Azure.Messaging.ServiceBus;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Channels;

namespace TrueVote.Api.Services
{
    public interface IServiceBus
    {
        Task SendAsync<T>(T message, string? subject = null, string? correlationId = null, string? queueName = null, CancellationToken cancellationToken = default) where T : class;
        Channel<FailedMessage> GetRetryChannel();
    }

    [ExcludeFromCodeCoverage]
    public class FailedMessage
    {
        public string Message { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string? CorrelationId { get; set; }
        public string QueueName { get; set; } = string.Empty;
        public DateTime FailedAt { get; set; }
        public int RetryCount { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public static class ServiceBusConstants
    {
        // Service Bus Client settings
        public static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(10);
        public static readonly int MaxClientRetries = 2;

        // Polly Retry settings
        public static readonly int PolicyRetryCount = 2;
        public static readonly TimeSpan PolicyRetryDelay = TimeSpan.FromSeconds(3);

        // Circuit Breaker settings
        public static readonly int CircuitBreakerExceptionsAllowed = 3;
        public static readonly TimeSpan CircuitBreakerDuration = TimeSpan.FromSeconds(30);

        // Message settings
        public static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(30);

        // Background Service settings
        public static readonly int MaxRetryAttempts = 5;
        public static readonly TimeSpan BackgroundServiceErrorDelay = TimeSpan.FromSeconds(5);
    }

    [ExcludeFromCodeCoverage]
    public class ResilientServiceBus : IServiceBus
    {
        private readonly IConfiguration _configuration;
        private readonly ServiceBusClient? _client;
        private readonly string _defaultQueueName;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly ILogger<ResilientServiceBus> _logger;
        private readonly Channel<FailedMessage> _retryChannel;

        public ResilientServiceBus(IConfiguration configuration, ILogger<ResilientServiceBus> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var clientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpTcp,
                RetryOptions = new ServiceBusRetryOptions
                {
                    MaxRetries = ServiceBusConstants.MaxClientRetries,
                    MaxDelay = ServiceBusConstants.MaxDelay,
                    Mode = ServiceBusRetryMode.Fixed
                }
            };

            var connectionString = _configuration.GetConnectionString("ServiceBusConnectionString");
            if (!string.IsNullOrEmpty(connectionString))
            {
                _client = new ServiceBusClient(connectionString, clientOptions);
            }
            _defaultQueueName = configuration["ServiceBusApiEventQueueName"]!;

            _retryChannel = Channel.CreateUnbounded<FailedMessage>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true
            });

            _retryPolicy = Policy
                .Handle<ServiceBusException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    ServiceBusConstants.PolicyRetryCount,
                    _ => ServiceBusConstants.PolicyRetryDelay,
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Error sending message to Service Bus. Retry attempt {retryCount} after {timeSpan.TotalSeconds}s. Error: {exception.Message}");
                    });

            _circuitBreakerPolicy = Policy
                .Handle<ServiceBusException>()
                .Or<TimeoutException>()
                .CircuitBreakerAsync(
                    ServiceBusConstants.CircuitBreakerExceptionsAllowed,
                    ServiceBusConstants.CircuitBreakerDuration,
                    onBreak: (exception, duration) =>
                    {
                        _logger.LogError($"Circuit breaker opened. Duration: {duration.TotalSeconds}s. Error: {exception.Message}");
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset");
                    });
        }

        public async Task SendAsync<T>(T message, string? subject = null, string? correlationId = null, string? queueName = null, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var targetQueue = queueName ?? _defaultQueueName;
                var sender = _client?.CreateSender(targetQueue);

                if (sender == null)
                {
                    throw new InvalidOperationException("Service Bus client not initialized");
                }

                var messageBody = message is string stringMessage
                    ? BinaryData.FromString(stringMessage)
                    : BinaryData.FromString(JsonSerializer.Serialize(message));

                var serviceBusMessage = new ServiceBusMessage(messageBody)
                {
                    Subject = subject,
                    CorrelationId = correlationId,
                };

                // If no cancellation token provided, create one with default timeout
                using var timeoutCts = new CancellationTokenSource();
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                // Only set timeout if none was provided
                if (!cancellationToken.CanBeCanceled)
                {
                    timeoutCts.CancelAfter(ServiceBusConstants.OperationTimeout);
                }

                await Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy)
                    .ExecuteAsync(async () =>
                    {
                        await sender.SendMessageAsync(serviceBusMessage, linkedCts.Token);
                    });
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Message sending timed out");
                await StoreFailedMessageAsync(message, subject, correlationId, queueName ?? _defaultQueueName);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Circuit breaker is open. Message sending skipped.");
                await StoreFailedMessageAsync(message, subject, correlationId, queueName ?? _defaultQueueName);
            }
            catch (Exception ex) when (ex is ServiceBusException or TimeoutException)
            {
                _logger.LogError(ex, "Failed to send message to Service Bus after all retries");
                await StoreFailedMessageAsync(message, subject, correlationId, queueName ?? _defaultQueueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending message to Service Bus");
                throw;
            }
        }

        private async Task StoreFailedMessageAsync<T>(T message, string? subject, string? correlationId, string queueName)
        {
            var failedMessage = new FailedMessage
            {
                Message = JsonSerializer.Serialize(message),
                Subject = subject,
                CorrelationId = correlationId,
                QueueName = queueName,
                FailedAt = DateTime.UtcNow,
                RetryCount = 0
            };

            await _retryChannel.Writer.WriteAsync(failedMessage);
            _logger.LogWarning($"Message stored for retry: {correlationId}");
        }

        public Channel<FailedMessage> GetRetryChannel()
        {
            return _retryChannel;
        }
    }

    [ExcludeFromCodeCoverage]
    public class RetryBackgroundService : BackgroundService
    {
        private readonly IServiceBus _serviceBus;
        private readonly Channel<FailedMessage> _retryChannel;
        private readonly ILogger<RetryBackgroundService> _logger;

        public RetryBackgroundService(IServiceBus serviceBus, ILogger<RetryBackgroundService> logger)
        {
            _serviceBus = serviceBus;
            _retryChannel = serviceBus.GetRetryChannel();
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await foreach (var failedMessage in _retryChannel.Reader.ReadAllAsync(stoppingToken))
                    {
                        if (failedMessage.RetryCount >= ServiceBusConstants.MaxRetryAttempts)
                        {
                            _logger.LogError($"Message {failedMessage.CorrelationId} exceeded maximum retry attempts");
                            continue;
                        }

                        var delaySeconds = Math.Pow(2, failedMessage.RetryCount);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);

                        try
                        {
                            var messageObj = JsonSerializer.Deserialize<object>(failedMessage.Message);
                            if (messageObj != null)
                            {
                                await _serviceBus.SendAsync(messageObj, failedMessage.Subject, failedMessage.CorrelationId, failedMessage.QueueName);

                                _logger.LogInformation($"Successfully retried message {failedMessage.CorrelationId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            failedMessage.RetryCount++;
                            _logger.LogError(ex, $"Error retrying message {failedMessage.CorrelationId}. Attempt {failedMessage.RetryCount}");

                            if (failedMessage.RetryCount < ServiceBusConstants.MaxRetryAttempts)
                            {
                                await _retryChannel.Writer.WriteAsync(failedMessage, stoppingToken);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in retry background service");

                    await Task.Delay(ServiceBusConstants.BackgroundServiceErrorDelay, stoppingToken);
                }
            }
        }
    }
}

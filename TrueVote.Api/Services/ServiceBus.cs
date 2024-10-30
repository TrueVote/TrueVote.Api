using Azure.Messaging.ServiceBus;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace TrueVote.Api.Services
{
    public interface IServiceBus
    {
        Task SendAsync<T>(T message, string? subject = null, string? correlationId = null, string? queueName = null) where T : class;
    }

    [ExcludeFromCodeCoverage]
    public class ServiceBus : IServiceBus
    {
        readonly IConfiguration _configuration;
        readonly ServiceBusClient _client;
        readonly string _defaultQueueName;

        public ServiceBus(IConfiguration configuration)
        {
            _configuration = configuration;
            var connectionString = _configuration.GetConnectionString("ServiceBusConnectionString");
            _client = new ServiceBusClient(connectionString);
            _defaultQueueName = _configuration["ServiceBusApiEventQueueName"]!;
        }

        public async Task SendAsync<T>(T message, string? subject = null, string? correlationId = null, string? queueName = null) where T : class
        {
            var targetQueue = queueName ?? _defaultQueueName;
            var sender = _client.CreateSender(targetQueue);

            var serviceBusMessage = new ServiceBusMessage(BinaryData.FromString(JsonSerializer.Serialize(message)))
            {
                Subject = subject,
                CorrelationId = correlationId
            };

            await sender.SendMessageAsync(serviceBusMessage);
        }
    }
}

using Azure.Messaging.ServiceBus;
using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api2.Services
{
    public interface IServiceBus
    {
        Task SendAsync(string message);
    }

    [ExcludeFromCodeCoverage]
    public class ServiceBus : IServiceBus
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusSender _serviceBusSender;
        private readonly IConfiguration _configuration;

        public ServiceBus(IConfiguration configuration)
        {
            _configuration = configuration;

            var connectionString = _configuration["ServiceBusConnectionString"];
            var queueName = _configuration["ServiceBusApiEventQueueName"];

            _serviceBusClient = new ServiceBusClient(connectionString);
            _serviceBusSender = _serviceBusClient.CreateSender(queueName);
        }

        public async Task SendAsync(string message)
        {
            var serviceBusMessage = new ServiceBusMessage(message);
            await _serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
    }
}

using Azure.Messaging.ServiceBus;
using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Services
{
    public interface IServiceBus
    {
        Task SendAsync(string message);
    }

    [ExcludeFromCodeCoverage]
    public class ServiceBus : IServiceBus
    {
        private readonly IConfiguration _configuration;

        public ServiceBus(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string message)
        {
            var connectionString = _configuration.GetConnectionString("ServiceBusConnectionString");
            var queueName = _configuration["ServiceBusApiEventQueueName"];

            var serviceBusClient = new ServiceBusClient(connectionString);
            var serviceBusSender = serviceBusClient.CreateSender(queueName);

            var serviceBusMessage = new ServiceBusMessage(message);
            await serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
    }
}

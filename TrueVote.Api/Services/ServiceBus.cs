using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;

namespace TrueVote.Api.Services
{
    public interface IServiceBus
    {
        Task SendAsync(string message);
    }

    public class ServiceBus : IServiceBus
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusSender _serviceBusSender;

        public ServiceBus()
        {
            var connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            var queueName = Environment.GetEnvironmentVariable("ServiceBusApiEventQueueName");

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

using Azure.Messaging.ServiceBus;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace TrueVote.Api.Services
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

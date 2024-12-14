
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace BankingApi.EventReceiver
{
    public class ServiceBusReceiver : IServiceBusReceiver
    {
        private readonly Queue<EventMessage> _messageQueue = new();
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;
        private readonly string _queueName;

        public ServiceBusReceiver(IConfiguration configuration)
        {
            // Get Service Bus connection string and queue name from configuration
            var serviceBusConnectionString = configuration.GetConnectionString("ServiceBusConnection");
            _queueName = configuration["ServiceBusQueueName"];

            // Initialize the ServiceBusClient and ServiceBusProcessor
            _client = new ServiceBusClient(serviceBusConnectionString);
            _processor = _client.CreateProcessor(_queueName, new ServiceBusProcessorOptions());
            _processor.ProcessMessageAsync += OnMessageReceived;
            _processor.ProcessErrorAsync += OnErrorReceived;
        }

        private Task OnMessageReceived(ProcessMessageEventArgs processMessageEventArgs)
        {
            // Logic to process messages from Azure Service Bus
            var message = processMessageEventArgs.Message;
            var eventMessage = new EventMessage
            {
                Id = Guid.NewGuid(),
                MessageBody = message.Body.ToString(),
                ProcessingCount = 0
            };
            _messageQueue.Enqueue(eventMessage);

            // Complete the message so it is removed from the queue
            return processMessageEventArgs.CompleteMessageAsync(message);
        }

        private Task OnErrorReceived(ProcessErrorEventArgs processMessageEventArgs)
        {
            // Handle error (log or reprocess)
            return Task.CompletedTask;
        }

        //Used to manually test the queue
        public Task AddMessageToQueue(EventMessage message)
        {
            _messageQueue.Enqueue(message);
            return Task.CompletedTask;
        }

        public Task<EventMessage?> Peek()
        {
            return Task.FromResult(_messageQueue.Count > 0 ? _messageQueue.Peek() : null);
        }

        public Task Abandon(EventMessage message)
        {
            message.ProcessingCount++;
            return Task.CompletedTask;
        }

        public Task Complete(EventMessage message)
        {
            _messageQueue.Dequeue();
            return Task.CompletedTask;
        }

        public Task MoveToDeadLetter(EventMessage message)
        {
            return Task.CompletedTask;
        }        

        public Task ReSchedule(EventMessage message, DateTime nextAvailableTime)
        {
            return Task.CompletedTask;
        }
    }
}

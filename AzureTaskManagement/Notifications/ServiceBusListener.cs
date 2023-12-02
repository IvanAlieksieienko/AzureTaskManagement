using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.SignalR;

namespace AzureTaskManagement.Notifications;

public class ServiceBusListener : BackgroundService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly string _connectionString;

    public ServiceBusListener(IHubContext<NotificationHub> hubContext, IConfiguration configuration)
    {
        _hubContext = hubContext;
        _connectionString = configuration["ServiceBus:ConnectionString"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create a Service Bus client
        var serviceBusClient = new ServiceBusClient(_connectionString);

        // Create a processor that we can use to process the messages
        var processor = serviceBusClient.CreateProcessor("notifiications");

        // Add handler to process messages
        processor.ProcessMessageAsync += async args =>
        {
            string body = args.Message.Body.ToString();
            // Send the notification to all connected SignalR clients
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", body);
            // Complete the message so that it is not received again
            await args.CompleteMessageAsync(args.Message);
        };

        // Add error handling
        processor.ProcessErrorAsync += args =>
        {
            // Handle the error
            return Task.CompletedTask;
        };

        // Start processing
        await processor.StartProcessingAsync();
    }
}
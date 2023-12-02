using Azure.Messaging.ServiceBus;

namespace AzureTaskManagement.Notifications;

public interface IServiceBusSender
{
    Task PutMessage(string message);
}

public class ServiceBusSender : IServiceBusSender
{
    private readonly string _connectionString;

    public ServiceBusSender(IConfiguration configuration)
    {
        _connectionString = configuration["ServiceBus:ConnectionString"];
    }

    public async Task PutMessage(string message)
    {
        ServiceBusClient client = new(_connectionString);
        Azure.Messaging.ServiceBus.ServiceBusSender sender = client.CreateSender("notifiications");
        try
        {
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(message);
            await sender.SendMessageAsync(serviceBusMessage);
        }
        finally
        {
            await client.DisposeAsync();
            await sender.DisposeAsync();
        }
    }
}
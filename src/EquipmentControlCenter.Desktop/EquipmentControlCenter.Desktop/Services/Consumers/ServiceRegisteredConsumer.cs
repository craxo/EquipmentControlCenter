using System.Threading.Tasks;
using EquipmentControlCenter.Shared.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EquipmentControlCenter.Desktop.Services.Consumers;

public class ServiceRegisteredConsumer : IConsumer<ServiceRegistered>
{
    private readonly EquipmentServiceRegistry _registry;
    private readonly ILogger<ServiceRegisteredConsumer> _logger;

    public ServiceRegisteredConsumer(
        EquipmentServiceRegistry registry,
        ILogger<ServiceRegisteredConsumer> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ServiceRegistered> context)
    {
        var message = context.Message;
        _logger.LogInformation("Service registered: {ServiceId} - {ServiceName}",
            message.ServiceId, message.ServiceName);

        _registry.RegisterService(message);
        return Task.CompletedTask;
    }
}

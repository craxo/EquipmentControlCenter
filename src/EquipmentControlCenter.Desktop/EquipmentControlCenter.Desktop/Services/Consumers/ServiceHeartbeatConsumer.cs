using System.Threading.Tasks;
using EquipmentControlCenter.Shared.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EquipmentControlCenter.Desktop.Services.Consumers;

public class ServiceHeartbeatConsumer : IConsumer<ServiceHeartbeat>
{
    private readonly EquipmentServiceRegistry _registry;
    private readonly ILogger<ServiceHeartbeatConsumer> _logger;

    public ServiceHeartbeatConsumer(
        EquipmentServiceRegistry registry,
        ILogger<ServiceHeartbeatConsumer> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ServiceHeartbeat> context)
    {
        var message = context.Message;
        _logger.LogDebug("Heartbeat received from {ServiceId}", message.ServiceId);

        _registry.UpdateHeartbeat(message);
        return Task.CompletedTask;
    }
}

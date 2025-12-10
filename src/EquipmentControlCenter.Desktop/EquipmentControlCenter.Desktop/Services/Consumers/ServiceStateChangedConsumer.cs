using System.Threading.Tasks;
using EquipmentControlCenter.Shared.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EquipmentControlCenter.Desktop.Services.Consumers;

public class ServiceStateChangedConsumer : IConsumer<ServiceStateChanged>
{
    private readonly EquipmentServiceRegistry _registry;
    private readonly ILogger<ServiceStateChangedConsumer> _logger;

    public ServiceStateChangedConsumer(
        EquipmentServiceRegistry registry,
        ILogger<ServiceStateChangedConsumer> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ServiceStateChanged> context)
    {
        var message = context.Message;
        _logger.LogInformation("State changed for {ServiceId}: {Key} = {Value}",
            message.ServiceId, message.StateKey, message.NewValue);

        _registry.UpdateState(message);
        return Task.CompletedTask;
    }
}

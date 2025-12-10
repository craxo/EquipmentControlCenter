using EquipmentControlCenter.Shared.Messages;
using MassTransit;

namespace EquipmentControlCenter.CncService.Services;

/// <summary>
/// Publishes service registration message on startup
/// </summary>
public class ServiceRegistrationPublisher : IHostedService
{
    private readonly IBus _bus;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceRegistrationPublisher> _logger;

    public ServiceRegistrationPublisher(
        IBus bus,
        IConfiguration configuration,
        ILogger<ServiceRegistrationPublisher> logger)
    {
        _bus = bus;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Small delay to ensure RabbitMQ connection is established
        await Task.Delay(2000, cancellationToken);

        var registration = new ServiceRegistered
        {
            ServiceId = _configuration["ServiceConfig:ServiceId"] ?? $"cnc-{Guid.NewGuid()}",
            ServiceName = _configuration["ServiceConfig:ServiceName"] ?? "cnc-service",
            MachineName = Environment.MachineName,
            EquipmentType = "CNC",
            Manufacturer = "Generic",
            Version = "1.0.0",
            RegisteredAt = DateTime.UtcNow,
            AvailableControls = CncControlDefinitionProvider.GetControlDefinitions(),
            Metadata = new Dictionary<string, string>
            {
                { "location", "Shop Floor 1" },
                { "capabilities", "3-axis milling" }
            }
        };

        await _bus.Publish(registration, cancellationToken);
        _logger.LogInformation("Service registration published for {ServiceId}", registration.ServiceId);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

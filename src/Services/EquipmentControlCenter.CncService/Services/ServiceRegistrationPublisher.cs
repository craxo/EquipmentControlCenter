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
    private readonly ServiceStateManager _stateManager;

    public ServiceRegistrationPublisher(
        IBus bus,
        IConfiguration configuration,
        ILogger<ServiceRegistrationPublisher> logger,
        ServiceStateManager stateManager)
    {
        _bus = bus;
        _configuration = configuration;
        _logger = logger;
        _stateManager = stateManager;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Small delay to ensure RabbitMQ connection is established
        await Task.Delay(2000, cancellationToken);

        // Publish initial state for all controls
        await PublishInitialStateAsync();

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

    private async Task PublishInitialStateAsync()
    {
        // Set initial state for all controls with default values
        await _stateManager.SetStateAsync("spindle-speed", 1000.0, "Initial state");
        await _stateManager.SetStateAsync("feed-rate", 50.0, "Initial state");
        await _stateManager.SetStateAsync("coolant-toggle", false, "Initial state");
        await _stateManager.SetStateAsync("program-name", "", "Initial state");
        await _stateManager.SetStateAsync("machine-status", "Stopped", "Initial state");

        _logger.LogInformation("Initial state published for all controls");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

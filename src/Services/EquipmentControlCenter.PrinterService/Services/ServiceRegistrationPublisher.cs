using EquipmentControlCenter.Shared.Messages;
using MassTransit;

namespace EquipmentControlCenter.PrinterService.Services;

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
        await Task.Delay(2000, cancellationToken);

        // Publish initial state for all controls
        await PublishInitialStateAsync();

        var registration = new ServiceRegistered
        {
            ServiceId = _configuration["ServiceConfig:ServiceId"] ?? $"printer-{Guid.NewGuid()}",
            ServiceName = _configuration["ServiceConfig:ServiceName"] ?? "printer-service",
            MachineName = Environment.MachineName,
            EquipmentType = "3D Printer",
            Manufacturer = "Generic",
            Version = "1.0.0",
            RegisteredAt = DateTime.UtcNow,
            AvailableControls = PrinterControlDefinitionProvider.GetControlDefinitions(),
            Metadata = new Dictionary<string, string>
            {
                { "location", "Lab 2" },
                { "capabilities", "FDM printing" }
            }
        };

        await _bus.Publish(registration, cancellationToken);
        _logger.LogInformation("Service registration published for {ServiceId}", registration.ServiceId);
    }

    private async Task PublishInitialStateAsync()
    {
        // Set initial state for all controls with default values
        await _stateManager.SetStateAsync("bed-temperature", 60.0, "Initial state");
        await _stateManager.SetStateAsync("nozzle-temperature", 200.0, "Initial state");
        await _stateManager.SetStateAsync("print-speed", 100.0, "Initial state");
        await _stateManager.SetStateAsync("filament-type", "PLA", "Initial state");
        await _stateManager.SetStateAsync("printer-status", "Idle", "Initial state");

        _logger.LogInformation("Initial state published for all controls");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

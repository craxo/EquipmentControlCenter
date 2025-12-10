using EquipmentControlCenter.Shared.Messages;
using MassTransit;

namespace EquipmentControlCenter.PrinterService.Services;

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
        await Task.Delay(2000, cancellationToken);

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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

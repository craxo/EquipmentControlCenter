using EquipmentControlCenter.Shared.Messages;
using MassTransit;

namespace EquipmentControlCenter.PrinterService.Services;

public class PrinterMonitoringService : BackgroundService
{
    private readonly ILogger<PrinterMonitoringService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IBus _bus;
    private readonly ServiceStateManager _stateManager;
    private readonly DateTime _startTime;
    private long _messageCount;

    public PrinterMonitoringService(
        ILogger<PrinterMonitoringService> logger,
        IConfiguration configuration,
        IBus bus,
        ServiceStateManager stateManager)
    {
        _logger = logger;
        _configuration = configuration;
        _bus = bus;
        _stateManager = stateManager;
        _startTime = DateTime.UtcNow;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("3D Printer Monitoring Service starting...");

        await InitializeStateAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishHeartbeatAsync();
                await Task.Delay(5000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in printer monitoring loop");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("3D Printer Monitoring Service stopped");
    }

    private async Task InitializeStateAsync()
    {
        await _stateManager.SetStateAsync("printer-status", "Idle", "Service started");
        await _stateManager.SetStateAsync("bed-temperature", 0.0, "Initial state");
        await _stateManager.SetStateAsync("nozzle-temperature", 0.0, "Initial state");
        await _stateManager.SetStateAsync("print-speed", 100.0, "Initial state");
        await _stateManager.SetStateAsync("filament-type", "PLA", "Initial state");
        await _stateManager.SetStateAsync("print-progress", 0.0, "Initial state");
    }

    private async Task PublishHeartbeatAsync()
    {
        var heartbeat = new ServiceHeartbeat
        {
            ServiceId = _configuration["ServiceConfig:ServiceId"] ?? $"printer-{Guid.NewGuid()}",
            ServiceName = _configuration["ServiceConfig:ServiceName"] ?? "printer-service",
            MachineName = Environment.MachineName,
            Timestamp = DateTime.UtcNow,
            HealthStatus = ServiceHealthStatus.Healthy,
            CurrentState = _stateManager.GetAllState(),
            Uptime = DateTime.UtcNow - _startTime,
            MessageCount = Interlocked.Increment(ref _messageCount)
        };

        await _bus.Publish(heartbeat);
        _logger.LogDebug("Heartbeat published: {MessageCount}", heartbeat.MessageCount);
    }
}

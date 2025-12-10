using Consul;
using EquipmentControlCenter.Shared.Messages;
using MassTransit;
using EquipmentControlCenter.CncService.Services;

namespace EquipmentControlCenter.CncService;

public class CncMonitoringService : BackgroundService
{
    private readonly ILogger<CncMonitoringService> _logger;
    private readonly ConsulConfigurationProvider _configProvider;
    private readonly IConfiguration _configuration;
    private readonly IBus _bus;
    private readonly ServiceStateManager _stateManager;
    private readonly DateTime _startTime;
    private long _messageCount;

    public CncMonitoringService(
        ILogger<CncMonitoringService> logger,
        ConsulConfigurationProvider configProvider,
        IConfiguration configuration,
        IBus bus,
        ServiceStateManager stateManager)
    {
        _logger = logger;
        _configProvider = configProvider;
        _configuration = configuration;
        _bus = bus;
        _stateManager = stateManager;
        _startTime = DateTime.UtcNow;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CNC Monitoring Service starting...");

        // Initialize default state
        await InitializeStateAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Publish heartbeat
                await PublishHeartbeatAsync();

                // Fetch polling interval from Consul (with default fallback)
                var intervalString = await _configProvider.GetConfigValueAsync("polling-interval");
                var interval = int.TryParse(intervalString, out var parsed)
                    ? parsed
                    : _configuration.GetValue<int>("CncSettings:PollingInterval", 5000);

                await Task.Delay(interval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CNC monitoring loop");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("CNC Monitoring Service stopped");
    }

    private async Task InitializeStateAsync()
    {
        await _stateManager.SetStateAsync("machine-status", "Idle", "Service started");
        await _stateManager.SetStateAsync("spindle-speed", 0.0, "Initial state");
        await _stateManager.SetStateAsync("feed-rate", 0.0, "Initial state");
        await _stateManager.SetStateAsync("coolant-enabled", false, "Initial state");
        await _stateManager.SetStateAsync("current-program", "", "Initial state");
    }

    private async Task PublishHeartbeatAsync()
    {
        var heartbeat = new ServiceHeartbeat
        {
            ServiceId = _configuration["ServiceConfig:ServiceId"] ?? $"cnc-{Guid.NewGuid()}",
            ServiceName = _configuration["ServiceConfig:ServiceName"] ?? "cnc-service",
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
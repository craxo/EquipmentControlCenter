using Consul;

namespace EquipmentControlCenter.CncService;

public class CncMonitoringService : BackgroundService
{
    private readonly ILogger<CncMonitoringService> _logger;
    private readonly ConsulConfigurationProvider _configProvider;
    private readonly IConfiguration _configuration;

    public CncMonitoringService(
        ILogger<CncMonitoringService> logger, 
        ConsulConfigurationProvider configProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _configProvider = configProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CNC Monitoring Service starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Fetch the message from Consul
                var message = await _configProvider.GetConfigValueAsync("display-message");
                
                if (!string.IsNullOrEmpty(message))
                {
                    Console.WriteLine($"[CNC Service] {message}");
                    _logger.LogInformation("Display message: {Message}", message);
                }
                else
                {
                    Console.WriteLine("[CNC Service] No message configured in Consul");
                }

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
}
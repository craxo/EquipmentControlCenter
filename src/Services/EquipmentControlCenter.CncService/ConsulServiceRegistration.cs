using Consul;

namespace EquipmentControlCenter.CncService;

public class ConsulServiceRegistration : IHostedService
{
    private readonly IConsulClient _consulClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConsulServiceRegistration> _logger;
    private string? _registrationId;

    public ConsulServiceRegistration(
        IConsulClient consulClient, 
        IConfiguration configuration,
        ILogger<ConsulServiceRegistration> logger)
    {
        _consulClient = consulClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var serviceName = _configuration["ServiceConfig:ServiceName"] ?? "cnc-service";
        var serviceId = _configuration["ServiceConfig:ServiceId"] ?? $"{serviceName}-{Guid.NewGuid()}";
        var serviceAddress = _configuration["ServiceConfig:ServiceAddress"] ?? "localhost";
        var servicePort = _configuration.GetValue<int>("ServiceConfig:ServicePort", 5001);

        _registrationId = serviceId;

        var registration = new AgentServiceRegistration
        {
            ID = serviceId,
            Name = serviceName,
            Address = serviceAddress,
            Port = servicePort,
            Tags = new[] { "equipment", "cnc", "integration" },
            Meta = new Dictionary<string, string>
            {
                { "equipment-type", "CNC" },
                { "manufacturer", "Generic" },
                { "version", "1.0.0" }
            },
            Check = new AgentServiceCheck
            {
                HTTP = $"http://{serviceAddress}:{servicePort}/health",
                Interval = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(5),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
            }
        };

        _logger.LogInformation("Registering service {ServiceId} with Consul", serviceId);

        // Retry logic for Consul registration
        int maxRetries = 5;
        int retryDelayMs = 2000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
                _logger.LogInformation("Service registered successfully with Consul");
                return;
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                _logger.LogWarning(ex, "Failed to register with Consul (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}ms...",
                    i + 1, maxRetries, retryDelayMs);
                await Task.Delay(retryDelayMs, cancellationToken);
            }
        }

        _logger.LogError("Failed to register with Consul after {MaxRetries} attempts. Service will continue without Consul registration.", maxRetries);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_registrationId != null)
        {
            try
            {
                _logger.LogInformation("Deregistering service {ServiceId} from Consul", _registrationId);
                await _consulClient.Agent.ServiceDeregister(_registrationId, cancellationToken);
                _logger.LogInformation("Service deregistered successfully from Consul");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deregister service {ServiceId} from Consul", _registrationId);
            }
        }
    }
}
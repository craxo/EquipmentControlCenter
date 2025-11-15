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
        await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
        _logger.LogInformation("Service registered successfully");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_registrationId != null)
        {
            _logger.LogInformation("Deregistering service {ServiceId} from Consul", _registrationId);
            await _consulClient.Agent.ServiceDeregister(_registrationId, cancellationToken);
        }
    }
}
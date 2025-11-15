using Consul;
using EquipmentControlCenter.CncService;

var builder = WebApplication.CreateBuilder(args);

// Add Consul client
builder.Services.AddSingleton<IConsulClient>(sp =>
{
    var consulHost = builder.Configuration["Consul:Host"] ?? "localhost";
    var consulPort = builder.Configuration.GetValue<int>("Consul:Port", 8500);
    
    return new ConsulClient(config =>
    {
        config.Address = new Uri($"http://{consulHost}:{consulPort}");
    });
});

// Add configuration provider
builder.Services.AddSingleton<ConsulConfigurationProvider>(sp =>
{
    var consulClient = sp.GetRequiredService<IConsulClient>();
    var serviceName = builder.Configuration["ServiceConfig:ServiceName"] ?? "cnc-service";
    return new ConsulConfigurationProvider(consulClient, serviceName);
});

// Add hosted services
builder.Services.AddHostedService<ConsulServiceRegistration>();
builder.Services.AddHostedService<CncMonitoringService>();

// Add controllers for management API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "cnc-service" }));

// Configuration endpoints
app.MapGet("/api/config", async (ConsulConfigurationProvider configProvider) =>
{
    var message = await configProvider.GetConfigValueAsync("display-message");
    var interval = await configProvider.GetConfigValueAsync("polling-interval");
    
    return Results.Ok(new
    {
        displayMessage = message,
        pollingInterval = interval
    });
});

app.MapPut("/api/config", async (ConfigUpdate config, ConsulConfigurationProvider configProvider) =>
{
    if (!string.IsNullOrEmpty(config.DisplayMessage))
    {
        await configProvider.SetConfigValueAsync("display-message", config.DisplayMessage);
    }
    
    if (config.PollingInterval.HasValue)
    {
        await configProvider.SetConfigValueAsync("polling-interval", config.PollingInterval.Value.ToString());
    }
    
    return Results.Ok(new { message = "Configuration updated" });
});

app.MapControllers();

app.Run();

// DTOs
public record ConfigUpdate(string? DisplayMessage, int? PollingInterval);
using Consul;
using EquipmentControlCenter.CncService;
using EquipmentControlCenter.CncService.Services;
using EquipmentControlCenter.CncService.Consumers;
using MassTransit;

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

// Add service state manager
builder.Services.AddSingleton<ServiceStateManager>();

// Add control executor
builder.Services.AddSingleton<CncControlExecutor>();

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Add consumers
    x.AddConsumer<ControlCommandConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        // Get RabbitMQ connection string from Aspire
        var rabbitMqConnection = builder.Configuration.GetConnectionString("rabbitmq");

        if (!string.IsNullOrEmpty(rabbitMqConnection))
        {
            cfg.Host(new Uri(rabbitMqConnection));
        }
        else
        {
            // Fallback for local development
            cfg.Host("localhost", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });
        }

        // Configure endpoints
        cfg.ConfigureEndpoints(context);

        // Configure retry policy
        cfg.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
    });
});

// Add hosted services
builder.Services.AddHostedService<ConsulServiceRegistration>();
builder.Services.AddHostedService<ServiceRegistrationPublisher>();
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
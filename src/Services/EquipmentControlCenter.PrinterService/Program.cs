using EquipmentControlCenter.PrinterService;
using EquipmentControlCenter.PrinterService.Services;
using EquipmentControlCenter.PrinterService.Consumers;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add service state manager
builder.Services.AddSingleton<ServiceStateManager>();

// Add control executor
builder.Services.AddSingleton<PrinterControlExecutor>();

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
builder.Services.AddHostedService<ServiceRegistrationPublisher>();
builder.Services.AddHostedService<PrinterMonitoringService>();

// Add controllers for management API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "printer-service" }));

app.MapControllers();

app.Run();

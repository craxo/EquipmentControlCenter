var builder = DistributedApplication.CreateBuilder(args);

var consul = builder.AddContainer("consul", "hashicorp/consul", "latest")
    .WithArgs("agent", "-dev", "-ui", "-client=0.0.0.0")
    .WithHttpEndpoint(8500, 8500, "http")
    .WithEndpoint(8600, 8600, "dns")
    .WithLifetime(ContainerLifetime.Persistent);   

var username = builder.AddParameter("username", "admin");
var password = builder.AddParameter("password", "admin");

var rabbitmq = builder.AddRabbitMQ("rabbitmq", username, password)
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

var cncService = builder.AddProject<Projects.EquipmentControlCenter_CncService>("cnc-service")
    .WaitFor(rabbitmq)
    .WaitFor(consul)
    .WithReference(rabbitmq)
    .WithEnvironment("Consul__Host", "localhost")
    .WithEnvironment("Consul__Port", "8500");

var printerService = builder.AddProject<Projects.EquipmentControlCenter_PrinterService>("printer-service")
    .WaitFor(rabbitmq)
    .WaitFor(consul)
    .WithReference(rabbitmq);

var desktopApp = builder.AddProject<Projects.EquipmentControlCenter_Desktop_Desktop>("desktop-ui")
    .WaitFor(rabbitmq)
    .WaitFor(consul)
    .WithReference(rabbitmq);

builder.Build().Run();

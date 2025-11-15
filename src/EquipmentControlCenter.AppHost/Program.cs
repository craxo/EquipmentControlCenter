var builder = DistributedApplication.CreateBuilder(args);

var consul = builder.AddContainer("consul", "hashicorp/consul", "latest")
    .WithArgs("agent", "-dev", "-ui", "-client=0.0.0.0")
    .WithHttpEndpoint(8500, 8500, "http")
    .WithEndpoint(8600, 8600, "dns")
    .WithLifetime(ContainerLifetime.Persistent);   

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

var cncService = builder.AddProject<Projects.EquipmentControlCenter_CncService>("cnc-service")
    .WithReference(rabbitmq) // RabbitMQ works with WithReference
    .WithEnvironment("Consul__Host", "localhost")
    .WithEnvironment("Consul__Port", "8500");

builder.Build().Run();

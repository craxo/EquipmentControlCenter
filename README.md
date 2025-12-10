# Equipment Control Center

A distributed real-time monitoring and control system for industrial equipment built with .NET 9, RabbitMQ, and Avalonia UI.

## Overview

Equipment Control Center is a modern microservices-based system that enables real-time monitoring and control of multiple equipment types (CNC machines, 3D printers, etc.) through a dynamic desktop interface. Services automatically discover each other, and the UI dynamically generates controls based on equipment capabilities.

## Key Features

- 🔄 **Automatic Service Discovery** - Services announce themselves; UI discovers them automatically
- ⚡ **Real-time Updates** - Hybrid messaging with periodic heartbeats + immediate state change events
- 🎛️ **Dynamic UI** - Control panels generated dynamically based on service capabilities
- 🔌 **Plug & Play** - Add new equipment services without modifying the UI
- 📊 **Observability** - Built-in distributed tracing, metrics, and logging via .NET Aspire
- 🐰 **Message Bus** - RabbitMQ with MassTransit for reliable messaging
- 🎯 **Type-Safe** - Strongly-typed message contracts shared across services

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                       AppHost (.NET Aspire)                  │
│  Orchestrates all services, containers, and dependencies    │
└─────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
    ┌───▼───┐           ┌─────▼─────┐        ┌─────▼─────┐
    │Consul │           │ RabbitMQ  │        │  Desktop  │
    │       │           │           │        │    UI     │
    └───────┘           └─────┬─────┘        └─────┬─────┘
                              │                    │
                    ┌─────────┴─────────┐          │
                    │                   │          │
              ┌─────▼─────┐      ┌──────▼──────┐   │
              │    CNC    │      │   Printer   │   │
              │  Service  │      │   Service   │   │
              └───────────┘      └─────────────┘   │
                    │                   │          │
                    └───────────────────┴──────────┘
                         (Pub/Sub Messages)
```

### Components

- **AppHost**: .NET Aspire orchestrator managing all services and infrastructure
- **Consul**: Service registry with health checks
- **RabbitMQ**: Message broker with MassTransit for pub/sub messaging
- **CNC Service**: Example equipment service (3-axis CNC machine)
- **Printer Service**: Example equipment service (3D printer)
- **Desktop UI**: Cross-platform Avalonia application with dynamic control generation
- **Shared**: Common message contracts and models

## Technologies

- **.NET 9.0** - Modern C# and latest framework features
- **Avalonia 11.3.8** - Cross-platform UI framework (Windows, Linux, macOS, Web)
- **MassTransit 8.3.4** - Distributed application framework for RabbitMQ
- **RabbitMQ 4.0** - Message broker with management plugin
- **Consul** - Service discovery and distributed configuration
- **.NET Aspire 9.0** - Cloud-ready stack for orchestration and observability
- **CommunityToolkit.Mvvm** - MVVM helpers for UI
- **OpenTelemetry** - Distributed tracing and metrics

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for Consul and RabbitMQ containers)
- Windows, Linux, or macOS

## Getting Started

### Quick Start (Run Everything)

The easiest way to run the entire system is using the AppHost:

```bash
cd src/EquipmentControlCenter.AppHost
dotnet run
```

This single command will:
1. Start Consul container (http://localhost:8500)
2. Start RabbitMQ container (http://localhost:15672)
3. Launch CNC Service
4. Launch 3D Printer Service
5. Open Desktop UI window
6. Open Aspire Dashboard in your browser

### Access Points

After starting the AppHost:

- **Aspire Dashboard**: http://localhost:15065 (check console for login URL with token)
- **Desktop UI**: Opens automatically as a native window
- **Consul UI**: http://localhost:8500
- **RabbitMQ Management**: http://localhost:15672 (username: `admin`, password: `admin`)

### Manual Start (Individual Services)

If you prefer to run services individually:

```bash
# Terminal 1: Start AppHost (infrastructure only)
cd src/EquipmentControlCenter.AppHost
dotnet run

# Terminal 2: Start CNC Service
cd src/Services/EquipmentControlCenter.CncService
dotnet run

# Terminal 3: Start Printer Service
cd src/Services/EquipmentControlCenter.PrinterService
dotnet run

# Terminal 4: Start Desktop UI
cd src/EquipmentControlCenter.Desktop/EquipmentControlCenter.Desktop.Desktop
dotnet run
```

## Project Structure

```
EquipmentControlCenter/
├── src/
│   ├── EquipmentControlCenter.AppHost/          # Aspire orchestrator
│   ├── EquipmentControlCenter.Shared/           # Message contracts
│   │   └── Messages/                            # Shared message types
│   ├── EquipmentControlCenter.ServiceDefaults/  # Common service configuration
│   ├── Services/
│   │   ├── EquipmentControlCenter.CncService/   # CNC machine service
│   │   └── EquipmentControlCenter.PrinterService/ # 3D printer service
│   ├── EquipmentControlCenter.Desktop/          # Avalonia UI (shared)
│   │   ├── EquipmentControlCenter.Desktop/      # Core UI logic
│   │   ├── EquipmentControlCenter.Desktop.Desktop/ # Desktop entry point
│   │   └── EquipmentControlCenter.Desktop.Browser/ # WebAssembly entry point
│   └── Infrastructure/
│       └── EquipmentControlCenter.ServiceDiscovery/ # Service discovery abstractions
└── tests/                                       # Unit and integration tests
```

## Message Flow

### Service Registration (On Startup)

```
1. Service starts → Publishes ServiceRegistered message
2. Desktop UI receives message → Adds service to list
3. Desktop UI displays service with all available controls
```

### Heartbeat (Every 5 seconds)

```
1. Service publishes ServiceHeartbeat with current state
2. Desktop UI receives → Updates health status, uptime, state
```

### State Change (Immediate)

```
1. User clicks control in UI → Sends ControlCommand
2. Service receives command → Executes action
3. Service updates state → Publishes ServiceStateChanged
4. Desktop UI receives → Updates control values in real-time
```

## Adding a New Equipment Service

1. **Create a new service project**:
   ```bash
   cd src/Services
   dotnet new webapi -n EquipmentControlCenter.LaserService
   ```

2. **Add references**:
   - Reference `EquipmentControlCenter.Shared`
   - Add MassTransit and MassTransit.RabbitMQ packages

3. **Copy the pattern** from CncService or PrinterService:
   - `ServiceStateManager` - State tracking
   - `ControlExecutor` - Business logic for controls
   - `ControlDefinitionProvider` - Define your controls
   - `ControlCommandConsumer` - Handle UI commands
   - `ServiceRegistrationPublisher` - Announce service on startup
   - `MonitoringService` - Send heartbeats

4. **Define your controls** in `ControlDefinitionProvider`:
   ```csharp
   new ControlDefinition
   {
       ControlId = "laser-power",
       DisplayName = "Laser Power",
       Type = ControlType.Slider,
       Category = "Laser Control",
       Constraints = new ControlConstraints
       {
           MinValue = 0,
           MaxValue = 100,
           Step = 1
       }
   }
   ```

5. **Add to AppHost**:
   ```csharp
   var laserService = builder.AddProject<Projects.EquipmentControlCenter_LaserService>("laser-service")
       .WaitFor(rabbitmq)
       .WithReference(rabbitmq);
   ```

6. **Run** - The service will automatically appear in the Desktop UI!

## Control Types

The system supports multiple control types:

| Type | Description | UI Component | Example |
|------|-------------|--------------|---------|
| `Button` | Execute action | Button | Start Machine, Emergency Stop |
| `Toggle` | On/Off switch | ToggleSwitch | Coolant On/Off |
| `Slider` | Numeric range | Slider + Apply Button | Spindle Speed (0-5000) |
| `TextInput` | Text entry | TextBox + Submit | Program Name |
| `NumericInput` | Number entry | NumericUpDown | Custom value |

## Configuration

### Service Configuration (appsettings.json)

```json
{
  "ServiceConfig": {
    "ServiceName": "my-service",
    "ServiceId": "my-service-001",
    "ServiceAddress": "localhost",
    "ServicePort": 5003
  }
}
```

### RabbitMQ Connection

Services automatically get RabbitMQ connection strings from Aspire via `GetConnectionString("rabbitmq")`.

For standalone mode, services fall back to `localhost:5672` with `guest/guest`.

## Observability

### Aspire Dashboard

The Aspire Dashboard provides:
- **Resources**: See status of all services and containers
- **Console Logs**: Real-time logs from each service
- **Structured Logs**: Filterable structured logging
- **Traces**: Distributed tracing across service boundaries
- **Metrics**: Performance metrics and health checks

### RabbitMQ Tracing

Enable message tracing:
```bash
docker exec $(docker ps -q -f name=rabbitmq) rabbitmq-plugins enable rabbitmq_tracing
```

Then access via RabbitMQ Management UI → Admin → Tracing.

## Message Contracts

All message contracts are defined in `EquipmentControlCenter.Shared/Messages/`:

- **ServiceRegistered** - Service announces capabilities
- **ServiceHeartbeat** - Periodic health and state
- **ServiceStateChanged** - Immediate state change notification
- **ServiceHealthReport** - Detailed health information
- **ControlDefinition** - Control metadata and constraints
- **ControlCommand** - Command from UI to service (request)
- **ControlCommandResponse** - Result of command execution (response)

## Development

### Build All Projects

```bash
cd src
dotnet build EquipmentControlCenter.slnx
```

### Run Tests

```bash
cd tests
dotnet test
```

### Clean Build

```bash
cd src
dotnet clean
dotnet build
```

## Troubleshooting

### Consul Connection Refused

**Symptom**: `No connection could be made because the target machine actively refused it (localhost:8500)`

**Solution**: Make sure AppHost is running first. It starts the Consul container.

### RabbitMQ Login Failed

**Symptom**: Can't log into RabbitMQ Management UI

**Solution**: Use credentials `admin/admin` (configured in AppHost). Default `guest/guest` only works from localhost.

### Desktop UI Not Showing Services

**Symptoms**: UI opens but service list is empty

**Solutions**:
1. Check RabbitMQ is running: `docker ps | grep rabbitmq`
2. Check service logs in Aspire Dashboard
3. Verify services published ServiceRegistered message in RabbitMQ Management UI

### Port Already in Use

**Symptom**: `Address already in use`

**Solution**: Stop existing containers and retry:
```bash
docker stop $(docker ps -q)
```

## Future Enhancements

- [ ] Multiple service instances (horizontal scaling)
- [ ] Historical data storage (time-series database)
- [ ] Real-time charting and visualization
- [ ] User authentication and authorization
- [ ] Role-based access control for controls
- [ ] Alert and notification system
- [ ] Service configuration UI
- [ ] Mobile app (iOS/Android via Avalonia)
- [ ] Web dashboard (WebAssembly)
- [ ] Integration with industrial protocols (OPC UA, Modbus)

## License

[Your License Here]

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Authors

- Your Name - Initial work

## Acknowledgments

- Built with [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/)
- UI powered by [Avalonia](https://avaloniaui.net/)
- Messaging via [MassTransit](https://masstransit.io/)

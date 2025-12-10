using System.Collections.Concurrent;
using EquipmentControlCenter.Shared.Messages;
using MassTransit;

namespace EquipmentControlCenter.PrinterService.Services;

public class ServiceStateManager
{
    private readonly IBus _bus;
    private readonly ILogger<ServiceStateManager> _logger;
    private readonly ConcurrentDictionary<string, object> _state = new();
    private readonly string _serviceId;
    private readonly string _serviceName;
    private readonly string _machineName;

    public ServiceStateManager(
        IBus bus,
        ILogger<ServiceStateManager> logger,
        IConfiguration configuration)
    {
        _bus = bus;
        _logger = logger;
        _serviceId = configuration["ServiceConfig:ServiceId"] ?? $"printer-{Guid.NewGuid()}";
        _serviceName = configuration["ServiceConfig:ServiceName"] ?? "printer-service";
        _machineName = Environment.MachineName;
    }

    public async Task SetStateAsync(string key, object value, string? reason = null)
    {
        var oldValue = _state.TryGetValue(key, out var old) ? old : null;

        if (oldValue?.Equals(value) == true)
            return;

        _state[key] = value;

        var stateChanged = new ServiceStateChanged
        {
            ServiceId = _serviceId,
            ServiceName = _serviceName,
            MachineName = _machineName,
            Timestamp = DateTime.UtcNow,
            StateKey = key,
            OldValue = oldValue ?? "null",
            NewValue = value,
            Reason = reason
        };

        await _bus.Publish(stateChanged);
        _logger.LogInformation("State changed: {Key} = {Value}", key, value);
    }

    public T? GetState<T>(string key)
    {
        return _state.TryGetValue(key, out var value) && value is T typedValue
            ? typedValue
            : default;
    }

    public Dictionary<string, object> GetAllState()
    {
        return new Dictionary<string, object>(_state);
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using EquipmentControlCenter.Shared.Messages;
using EquipmentControlCenter.Desktop.Models;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;

namespace EquipmentControlCenter.Desktop.Services;

/// <summary>
/// Maintains registry of all discovered equipment services
/// </summary>
public class EquipmentServiceRegistry
{
    private readonly Dictionary<string, EquipmentService> _services = new();
    private readonly ILogger<EquipmentServiceRegistry> _logger;

    public ObservableCollection<EquipmentService> Services { get; } = new();

    public event EventHandler<EquipmentService>? ServiceAdded;
    public event EventHandler<EquipmentService>? ServiceUpdated;

    public EquipmentServiceRegistry(ILogger<EquipmentServiceRegistry> logger)
    {
        _logger = logger;
    }

    public void RegisterService(ServiceRegistered message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_services.ContainsKey(message.ServiceId))
            {
                _logger.LogInformation("Service {ServiceId} already registered, updating", message.ServiceId);
                UpdateExistingService(message);
                return;
            }

            var service = new EquipmentService
            {
                ServiceId = message.ServiceId,
                ServiceName = message.ServiceName,
                MachineName = message.MachineName,
                EquipmentType = message.EquipmentType,
                Manufacturer = message.Manufacturer,
                HealthStatus = ServiceHealthStatus.Unknown,
                LastHeartbeat = message.RegisteredAt,
                AvailableControls = message.AvailableControls,
                Metadata = message.Metadata ?? new()
            };

            _services[message.ServiceId] = service;
            Services.Add(service);
            ServiceAdded?.Invoke(this, service);

            _logger.LogInformation("Service {ServiceId} registered with {ControlCount} controls",
                message.ServiceId, message.AvailableControls.Count);
        });
    }

    public void UpdateHeartbeat(ServiceHeartbeat message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!_services.TryGetValue(message.ServiceId, out var service))
            {
                _logger.LogWarning("Received heartbeat for unknown service {ServiceId}", message.ServiceId);
                return;
            }

            service.LastHeartbeat = message.Timestamp;
            service.HealthStatus = message.HealthStatus;
            service.Uptime = message.Uptime;
            service.CurrentState = message.CurrentState;

            ServiceUpdated?.Invoke(this, service);
        });
    }

    public void UpdateState(ServiceStateChanged message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!_services.TryGetValue(message.ServiceId, out var service))
            {
                _logger.LogWarning("Received state change for unknown service {ServiceId}", message.ServiceId);
                return;
            }

            service.CurrentState[message.StateKey] = message.NewValue;
            ServiceUpdated?.Invoke(this, service);
        });
    }

    private void UpdateExistingService(ServiceRegistered message)
    {
        var service = _services[message.ServiceId];
        service.ServiceName = message.ServiceName;
        service.MachineName = message.MachineName;
        service.EquipmentType = message.EquipmentType;
        service.Manufacturer = message.Manufacturer;
        service.AvailableControls = message.AvailableControls;
        service.Metadata = message.Metadata ?? new();

        ServiceUpdated?.Invoke(this, service);
    }

    public EquipmentService? GetService(string serviceId)
    {
        return _services.TryGetValue(serviceId, out var service) ? service : null;
    }
}

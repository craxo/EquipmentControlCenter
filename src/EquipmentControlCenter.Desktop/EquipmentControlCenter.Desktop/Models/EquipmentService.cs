using System;
using System.Collections.Generic;
using EquipmentControlCenter.Shared.Messages;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EquipmentControlCenter.Desktop.Models;

/// <summary>
/// Represents a registered equipment service
/// </summary>
public partial class EquipmentService : ObservableObject
{
    [ObservableProperty]
    private string _serviceId = string.Empty;

    [ObservableProperty]
    private string _serviceName = string.Empty;

    [ObservableProperty]
    private string _machineName = string.Empty;

    [ObservableProperty]
    private string _equipmentType = string.Empty;

    [ObservableProperty]
    private string _manufacturer = string.Empty;

    [ObservableProperty]
    private ServiceHealthStatus _healthStatus = ServiceHealthStatus.Unknown;

    [ObservableProperty]
    private DateTime _lastHeartbeat = DateTime.MinValue;

    [ObservableProperty]
    private TimeSpan _uptime;

    [ObservableProperty]
    private Dictionary<string, object> _currentState = new();

    public List<ControlDefinition> AvailableControls { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();

    public bool IsOnline => (DateTime.UtcNow - LastHeartbeat).TotalSeconds < 30;
}

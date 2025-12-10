namespace EquipmentControlCenter.Shared.Messages;

/// <summary>
/// Published when a service starts and registers itself with the system
/// </summary>
public record ServiceRegistered
{
    public required string ServiceId { get; init; }
    public required string ServiceName { get; init; }
    public required string MachineName { get; init; }
    public required string EquipmentType { get; init; }
    public required string Manufacturer { get; init; }
    public required string Version { get; init; }
    public required DateTime RegisteredAt { get; init; }
    public required List<ControlDefinition> AvailableControls { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}

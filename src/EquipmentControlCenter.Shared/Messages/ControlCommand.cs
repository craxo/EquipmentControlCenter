namespace EquipmentControlCenter.Shared.Messages;

/// <summary>
/// Request to execute a control command
/// </summary>
public record ControlCommand
{
    public required string ServiceId { get; init; }
    public required string ControlId { get; init; }
    public required string CommandId { get; init; } // Unique ID for tracking
    public required object Value { get; init; }
    public required DateTime RequestedAt { get; init; }
    public string? RequestedBy { get; init; }
    public Dictionary<string, object>? Parameters { get; init; }
}

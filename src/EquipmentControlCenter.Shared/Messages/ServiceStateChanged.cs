namespace EquipmentControlCenter.Shared.Messages;

/// <summary>
/// Published when service state changes (not periodic)
/// </summary>
public record ServiceStateChanged
{
    public required string ServiceId { get; init; }
    public required string ServiceName { get; init; }
    public required string MachineName { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string StateKey { get; init; }
    public required object OldValue { get; init; }
    public required object NewValue { get; init; }
    public string? Reason { get; init; }
    public Dictionary<string, object>? AdditionalContext { get; init; }
}

namespace EquipmentControlCenter.Shared.Messages;

/// <summary>
/// Response to a control command request
/// </summary>
public record ControlCommandResponse
{
    public required string CommandId { get; init; }
    public required string ServiceId { get; init; }
    public required string ControlId { get; init; }
    public required bool Success { get; init; }
    public required DateTime ProcessedAt { get; init; }
    public object? ResultValue { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
    public TimeSpan ProcessingTime { get; init; }
    public Dictionary<string, object>? AdditionalData { get; init; }
}

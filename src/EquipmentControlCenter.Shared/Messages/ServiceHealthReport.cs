namespace EquipmentControlCenter.Shared.Messages;

/// <summary>
/// Published when service health status changes or on request
/// </summary>
public record ServiceHealthReport
{
    public required string ServiceId { get; init; }
    public required string ServiceName { get; init; }
    public required DateTime Timestamp { get; init; }
    public required ServiceHealthStatus Status { get; init; }
    public required List<HealthCheck> Checks { get; init; }
    public string? Description { get; init; }
}

public record HealthCheck
{
    public required string Name { get; init; }
    public required ServiceHealthStatus Status { get; init; }
    public string? Description { get; init; }
    public TimeSpan? Duration { get; init; }
}

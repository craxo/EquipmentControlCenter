namespace EquipmentControlCenter.Shared.Messages;

/// <summary>
/// Published periodically to indicate service is alive and provide current state
/// </summary>
public record ServiceHeartbeat
{
    public required string ServiceId { get; init; }
    public required string ServiceName { get; init; }
    public required string MachineName { get; init; }
    public required DateTime Timestamp { get; init; }
    public required ServiceHealthStatus HealthStatus { get; init; }
    public required Dictionary<string, object> CurrentState { get; init; }
    public TimeSpan Uptime { get; init; }
    public long MessageCount { get; init; }
}

public enum ServiceHealthStatus
{
    Healthy,
    Degraded,
    Unhealthy,
    Unknown
}

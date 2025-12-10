namespace EquipmentControlCenter.CncService.Services;

/// <summary>
/// Executes CNC-specific control commands
/// </summary>
public class CncControlExecutor
{
    private readonly ILogger<CncControlExecutor> _logger;
    private readonly ServiceStateManager _stateManager;

    public CncControlExecutor(
        ILogger<CncControlExecutor> logger,
        ServiceStateManager stateManager)
    {
        _logger = logger;
        _stateManager = stateManager;
    }

    public async Task<object> ExecuteAsync(string controlId, object value)
    {
        _logger.LogInformation("Executing control {ControlId} with value {Value}", controlId, value);

        // Simulate control execution with actual state changes
        switch (controlId)
        {
            case "start-machine":
                await _stateManager.SetStateAsync("machine-status", "Running", "User command");
                await _stateManager.SetStateAsync("last-started", DateTime.UtcNow, "Start command");
                return "Machine started successfully";

            case "stop-machine":
                await _stateManager.SetStateAsync("machine-status", "Stopped", "User command");
                return "Machine stopped successfully";

            case "emergency-stop":
                await _stateManager.SetStateAsync("machine-status", "Emergency Stop", "Emergency button");
                await _stateManager.SetStateAsync("emergency-active", true, "User triggered");
                return "Emergency stop activated";

            case "spindle-speed":
                if (value is not double speed)
                    throw new ArgumentException("Spindle speed must be a number");

                await _stateManager.SetStateAsync("spindle-speed", speed, "User adjustment");
                return $"Spindle speed set to {speed} RPM";

            case "feed-rate":
                if (value is not double feedRate)
                    throw new ArgumentException("Feed rate must be a number");

                await _stateManager.SetStateAsync("feed-rate", feedRate, "User adjustment");
                return $"Feed rate set to {feedRate}";

            case "coolant-toggle":
                if (value is not bool coolantOn)
                    throw new ArgumentException("Coolant state must be boolean");

                await _stateManager.SetStateAsync("coolant-enabled", coolantOn, "User toggle");
                return coolantOn ? "Coolant enabled" : "Coolant disabled";

            case "program-name":
                if (value is not string programName)
                    throw new ArgumentException("Program name must be a string");

                await _stateManager.SetStateAsync("current-program", programName, "User input");
                return $"Program set to {programName}";

            default:
                throw new InvalidOperationException($"Unknown control: {controlId}");
        }
    }
}

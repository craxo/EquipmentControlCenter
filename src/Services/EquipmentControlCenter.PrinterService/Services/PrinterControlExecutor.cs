namespace EquipmentControlCenter.PrinterService.Services;

public class PrinterControlExecutor
{
    private readonly ILogger<PrinterControlExecutor> _logger;
    private readonly ServiceStateManager _stateManager;

    public PrinterControlExecutor(
        ILogger<PrinterControlExecutor> logger,
        ServiceStateManager stateManager)
    {
        _logger = logger;
        _stateManager = stateManager;
    }

    public async Task<object> ExecuteAsync(string controlId, object value)
    {
        _logger.LogInformation("Executing control {ControlId} with value {Value}", controlId, value);

        switch (controlId)
        {
            case "start-print":
                await _stateManager.SetStateAsync("printer-status", "Printing", "User command");
                await _stateManager.SetStateAsync("print-started", DateTime.UtcNow, "Start command");
                return "Print started successfully";

            case "pause-print":
                await _stateManager.SetStateAsync("printer-status", "Paused", "User command");
                return "Print paused";

            case "stop-print":
                await _stateManager.SetStateAsync("printer-status", "Stopped", "User command");
                await _stateManager.SetStateAsync("print-progress", 0.0, "Print stopped");
                return "Print stopped";

            case "bed-temperature":
                if (value is not double temp)
                    throw new ArgumentException("Bed temperature must be a number");

                await _stateManager.SetStateAsync("bed-temperature", temp, "User adjustment");
                return $"Bed temperature set to {temp}°C";

            case "nozzle-temperature":
                if (value is not double nozzleTemp)
                    throw new ArgumentException("Nozzle temperature must be a number");

                await _stateManager.SetStateAsync("nozzle-temperature", nozzleTemp, "User adjustment");
                return $"Nozzle temperature set to {nozzleTemp}°C";

            case "print-speed":
                if (value is not double speed)
                    throw new ArgumentException("Print speed must be a number");

                await _stateManager.SetStateAsync("print-speed", speed, "User adjustment");
                return $"Print speed set to {speed}%";

            case "filament-type":
                if (value is not string filament)
                    throw new ArgumentException("Filament type must be a string");

                await _stateManager.SetStateAsync("filament-type", filament, "User input");
                return $"Filament type set to {filament}";

            default:
                throw new InvalidOperationException($"Unknown control: {controlId}");
        }
    }
}

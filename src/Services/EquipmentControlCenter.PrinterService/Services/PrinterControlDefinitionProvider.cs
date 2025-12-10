using EquipmentControlCenter.Shared.Messages;

namespace EquipmentControlCenter.PrinterService.Services;

public static class PrinterControlDefinitionProvider
{
    public static List<ControlDefinition> GetControlDefinitions()
    {
        return new List<ControlDefinition>
        {
            new ControlDefinition
            {
                ControlId = "start-print",
                DisplayName = "Start Print",
                Type = ControlType.Button,
                Category = "Print Control",
                Description = "Start the 3D print job",
                DisplayOrder = 1,
                IsEnabled = true
            },
            new ControlDefinition
            {
                ControlId = "pause-print",
                DisplayName = "Pause Print",
                Type = ControlType.Button,
                Category = "Print Control",
                Description = "Pause the current print",
                DisplayOrder = 2,
                IsEnabled = true
            },
            new ControlDefinition
            {
                ControlId = "stop-print",
                DisplayName = "Stop Print",
                Type = ControlType.Button,
                Category = "Print Control",
                Description = "Stop and cancel the print job",
                DisplayOrder = 3,
                IsEnabled = true,
                Metadata = new Dictionary<string, string>
                {
                    { "style", "warning" }
                }
            },
            new ControlDefinition
            {
                ControlId = "bed-temperature",
                DisplayName = "Bed Temperature",
                Type = ControlType.Slider,
                Category = "Temperature Control",
                Description = "Set heated bed temperature in °C",
                CurrentValue = 60.0,
                DisplayOrder = 10,
                IsEnabled = true,
                Constraints = new ControlConstraints
                {
                    MinValue = 0,
                    MaxValue = 120,
                    Step = 5
                }
            },
            new ControlDefinition
            {
                ControlId = "nozzle-temperature",
                DisplayName = "Nozzle Temperature",
                Type = ControlType.Slider,
                Category = "Temperature Control",
                Description = "Set nozzle temperature in °C",
                CurrentValue = 200.0,
                DisplayOrder = 11,
                IsEnabled = true,
                Constraints = new ControlConstraints
                {
                    MinValue = 0,
                    MaxValue = 300,
                    Step = 5
                }
            },
            new ControlDefinition
            {
                ControlId = "print-speed",
                DisplayName = "Print Speed",
                Type = ControlType.Slider,
                Category = "Print Settings",
                Description = "Set print speed as percentage",
                CurrentValue = 100.0,
                DisplayOrder = 20,
                IsEnabled = true,
                Constraints = new ControlConstraints
                {
                    MinValue = 10,
                    MaxValue = 200,
                    Step = 10
                }
            },
            new ControlDefinition
            {
                ControlId = "filament-type",
                DisplayName = "Filament Type",
                Type = ControlType.TextInput,
                Category = "Print Settings",
                Description = "Current filament material type",
                CurrentValue = "PLA",
                DisplayOrder = 21,
                IsEnabled = true,
                Constraints = new ControlConstraints
                {
                    MaxLength = 20,
                    Pattern = "^[a-zA-Z0-9 ]+$",
                    ValidationMessage = "Only alphanumeric characters and spaces allowed"
                }
            }
        };
    }
}

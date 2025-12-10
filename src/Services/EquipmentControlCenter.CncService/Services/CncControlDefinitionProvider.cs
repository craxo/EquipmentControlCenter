using EquipmentControlCenter.Shared.Messages;

namespace EquipmentControlCenter.CncService.Services;

/// <summary>
/// Provides control definitions for the CNC service
/// </summary>
public static class CncControlDefinitionProvider
{
    public static List<ControlDefinition> GetControlDefinitions()
    {
        return new List<ControlDefinition>
        {
            new ControlDefinition
            {
                ControlId = "start-machine",
                DisplayName = "Start Machine",
                Type = ControlType.Button,
                Category = "Machine Control",
                Description = "Start the CNC machine",
                DisplayOrder = 1,
                IsEnabled = true
            },
            new ControlDefinition
            {
                ControlId = "stop-machine",
                DisplayName = "Stop Machine",
                Type = ControlType.Button,
                Category = "Machine Control",
                Description = "Stop the CNC machine",
                DisplayOrder = 2,
                IsEnabled = true
            },
            new ControlDefinition
            {
                ControlId = "emergency-stop",
                DisplayName = "Emergency Stop",
                Type = ControlType.Button,
                Category = "Machine Control",
                Description = "Emergency stop - immediately halt all operations",
                DisplayOrder = 3,
                IsEnabled = true,
                Metadata = new Dictionary<string, string>
                {
                    { "style", "danger" },
                    { "confirm", "true" }
                }
            },
            new ControlDefinition
            {
                ControlId = "spindle-speed",
                DisplayName = "Spindle Speed",
                Type = ControlType.Slider,
                Category = "Machine Parameters",
                Description = "Set spindle speed in RPM",
                CurrentValue = 1000.0,
                DisplayOrder = 10,
                IsEnabled = true,
                Constraints = new ControlConstraints
                {
                    MinValue = 0,
                    MaxValue = 5000,
                    Step = 100
                }
            },
            new ControlDefinition
            {
                ControlId = "feed-rate",
                DisplayName = "Feed Rate",
                Type = ControlType.Slider,
                Category = "Machine Parameters",
                Description = "Set feed rate",
                CurrentValue = 50.0,
                DisplayOrder = 11,
                IsEnabled = true,
                Constraints = new ControlConstraints
                {
                    MinValue = 0,
                    MaxValue = 200,
                    Step = 5
                }
            },
            new ControlDefinition
            {
                ControlId = "coolant-toggle",
                DisplayName = "Coolant",
                Type = ControlType.Toggle,
                Category = "Machine Parameters",
                Description = "Enable/disable coolant",
                CurrentValue = false,
                DisplayOrder = 12,
                IsEnabled = true
            },
            new ControlDefinition
            {
                ControlId = "program-name",
                DisplayName = "Program Name",
                Type = ControlType.TextInput,
                Category = "Program Control",
                Description = "Current program name",
                CurrentValue = "",
                DisplayOrder = 20,
                IsEnabled = true,
                Constraints = new ControlConstraints
                {
                    MaxLength = 50,
                    Pattern = "^[a-zA-Z0-9_-]+$",
                    ValidationMessage = "Only alphanumeric characters, hyphens, and underscores allowed"
                }
            }
        };
    }
}

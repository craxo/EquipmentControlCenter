namespace EquipmentControlCenter.Shared.Messages;

/// <summary>
/// Defines a control that should be displayed in the UI
/// </summary>
public record ControlDefinition
{
    public required string ControlId { get; init; }
    public required string DisplayName { get; init; }
    public required ControlType Type { get; init; }
    public required string Category { get; init; }
    public string? Description { get; init; }
    public object? CurrentValue { get; init; }
    public ControlConstraints? Constraints { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsEnabled { get; init; } = true;
    public Dictionary<string, string>? Metadata { get; init; }
}

public enum ControlType
{
    Button,
    Toggle,
    Slider,
    TextInput,
    NumericInput,
    Dropdown,
    MultiButton // For button groups
}

public record ControlConstraints
{
    // For Slider/NumericInput
    public double? MinValue { get; init; }
    public double? MaxValue { get; init; }
    public double? Step { get; init; }

    // For TextInput
    public int? MaxLength { get; init; }
    public string? Pattern { get; init; }

    // For Dropdown/MultiButton
    public List<string>? AllowedValues { get; init; }

    // For all types
    public bool IsRequired { get; init; }
    public string? ValidationMessage { get; init; }
}

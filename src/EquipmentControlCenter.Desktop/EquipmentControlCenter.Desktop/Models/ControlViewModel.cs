using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using EquipmentControlCenter.Shared.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EquipmentControlCenter.Desktop.Models;

/// <summary>
/// View model for a single control
/// </summary>
public partial class ControlViewModel : ObservableObject
{
    private readonly ControlDefinition _definition;
    private readonly Func<ControlViewModel, Task> _executeCommand;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StateIndicatorText))]
    [NotifyPropertyChangedFor(nameof(StateIndicatorBackground))]
    private object? _currentValue;

    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StateIndicatorText))]
    [NotifyPropertyChangedFor(nameof(StateIndicatorBackground))]
    private bool _isExecuting;

    public ControlViewModel(ControlDefinition definition, Func<ControlViewModel, Task> executeCommand)
    {
        _definition = definition;
        _executeCommand = executeCommand;
        _currentValue = definition.CurrentValue;
        _isEnabled = definition.IsEnabled;
    }

    public string ControlId => _definition.ControlId;
    public string DisplayName => _definition.DisplayName;
    public ControlType Type => _definition.Type;
    public string Category => _definition.Category;
    public string? Description => _definition.Description;
    public ControlConstraints? Constraints => _definition.Constraints;
    public int DisplayOrder => _definition.DisplayOrder;
    public Dictionary<string, string>? Metadata => _definition.Metadata;

    public ICommand ExecuteCommand => new AsyncRelayCommand(ExecuteAsync);

    private async Task ExecuteAsync()
    {
        if (!IsEnabled || IsExecuting)
            return;

        IsExecuting = true;
        try
        {
            await _executeCommand(this);
        }
        finally
        {
            IsExecuting = false;
        }
    }

    /// <summary>
    /// Gets the state indicator text based on control type and current value
    /// </summary>
    public string StateIndicatorText
    {
        get
        {
            // Show executing state for all types when executing
            if (IsExecuting)
                return "EXECUTING";

            return Type switch
            {
                ControlType.Toggle => GetToggleStateText(),
                ControlType.Button => CurrentValue?.ToString()?.ToUpperInvariant() ?? "READY",
                ControlType.Slider => CurrentValue?.ToString() ?? "0",
                ControlType.NumericInput => CurrentValue?.ToString() ?? "0",
                ControlType.Dropdown => CurrentValue?.ToString() ?? "N/A",
                ControlType.TextInput => string.IsNullOrEmpty(CurrentValue?.ToString()) ? "EMPTY" : CurrentValue.ToString() ?? "N/A",
                ControlType.MultiButton => CurrentValue?.ToString() ?? "N/A",
                _ => "N/A"
            };
        }
    }

    /// <summary>
    /// Gets the state indicator background color based on control type and current value
    /// </summary>
    public string StateIndicatorBackground
    {
        get
        {
            // Show orange when executing
            if (IsExecuting)
                return "#FF9800"; // Orange for executing

            if (CurrentValue == null && Type != ControlType.Button)
                return "#9E9E9E"; // Gray for unknown

            return Type switch
            {
                ControlType.Toggle => GetToggleStateColor(),
                ControlType.Button => GetButtonStateColor(),
                _ => "#607D8B" // Blue-gray for other types
            };
        }
    }

    private string GetToggleStateText()
    {
        if (CurrentValue == null)
            return "OFF";

        // Handle boolean values
        if (CurrentValue is bool boolValue)
            return boolValue ? "ON" : "OFF";

        // Handle string values
        var stringValue = CurrentValue.ToString()?.ToUpperInvariant();
        if (stringValue == "TRUE" || stringValue == "ON" || stringValue == "1")
            return "ON";
        if (stringValue == "FALSE" || stringValue == "OFF" || stringValue == "0")
            return "OFF";

        return CurrentValue.ToString()?.ToUpperInvariant() ?? "OFF";
    }

    private string GetToggleStateColor()
    {
        if (CurrentValue == null)
            return "#9E9E9E"; // Gray

        // Handle boolean values
        if (CurrentValue is bool boolValue)
            return boolValue ? "#4CAF50" : "#757575"; // Green for ON, Gray for OFF

        // Handle string values
        var stringValue = CurrentValue.ToString()?.ToUpperInvariant();
        if (stringValue == "TRUE" || stringValue == "ON" || stringValue == "1")
            return "#4CAF50"; // Green for ON
        if (stringValue == "FALSE" || stringValue == "OFF" || stringValue == "0")
            return "#757575"; // Gray for OFF

        return "#9E9E9E"; // Gray for unknown
    }

    private string GetButtonStateColor()
    {
        if (CurrentValue == null)
            return "#2196F3"; // Blue for ready

        var stateValue = CurrentValue.ToString()?.ToUpperInvariant();

        return stateValue switch
        {
            "COMPLETED" or "SUCCESS" or "DONE" => "#4CAF50", // Green for success
            "RUNNING" or "ACTIVE" => "#FF9800", // Orange for active
            "ERROR" or "FAILED" => "#F44336", // Red for error
            "READY" => "#2196F3", // Blue for ready
            _ => "#2196F3" // Default blue
        };
    }
}

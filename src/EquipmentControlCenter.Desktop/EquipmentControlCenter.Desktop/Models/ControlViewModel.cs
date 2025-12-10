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
    private object? _currentValue;

    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
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
}

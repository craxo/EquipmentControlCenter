using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentControlCenter.Desktop.Models;
using EquipmentControlCenter.Desktop.Services;

namespace EquipmentControlCenter.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly EquipmentServiceRegistry _registry;
    private readonly ControlCommandService _commandService;

    [ObservableProperty]
    private EquipmentService? _selectedService;

    [ObservableProperty]
    private ObservableCollection<ControlViewModel> _availableControls = new();

    public ObservableCollection<EquipmentService> Services => _registry.Services;

    public MainViewModel(
        EquipmentServiceRegistry registry,
        ControlCommandService commandService)
    {
        _registry = registry;
        _commandService = commandService;

        _registry.ServiceAdded += OnServiceAdded;
        _registry.ServiceUpdated += OnServiceUpdated;
    }

    partial void OnSelectedServiceChanged(EquipmentService? value)
    {
        LoadControlsForService(value);
    }

    private void LoadControlsForService(EquipmentService? service)
    {
        AvailableControls.Clear();

        if (service == null)
            return;

        var controlViewModels = service.AvailableControls
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new ControlViewModel(c, ExecuteControlAsync))
            .ToList();

        foreach (var vm in controlViewModels)
        {
            AvailableControls.Add(vm);
        }
    }

    private async Task ExecuteControlAsync(ControlViewModel control)
    {
        if (SelectedService == null)
            return;

        try
        {
            var response = await _commandService.SendCommandAsync(
                SelectedService.ServiceId,
                control.ControlId,
                control.CurrentValue ?? string.Empty);

            if (!response.Success)
            {
                // Handle error - could show notification
                // For now, log to console
                System.Diagnostics.Debug.WriteLine($"Command failed: {response.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error executing command: {ex.Message}");
        }
    }

    private void OnServiceAdded(object? sender, EquipmentService service)
    {
        // Auto-select first service
        if (SelectedService == null)
        {
            SelectedService = service;
        }
    }

    private void OnServiceUpdated(object? sender, EquipmentService service)
    {
        // Update control values if this is the selected service
        if (SelectedService?.ServiceId == service.ServiceId)
        {
            foreach (var control in AvailableControls)
            {
                if (service.CurrentState.TryGetValue(control.ControlId, out var value))
                {
                    control.CurrentValue = value;
                }
            }
        }
    }
}

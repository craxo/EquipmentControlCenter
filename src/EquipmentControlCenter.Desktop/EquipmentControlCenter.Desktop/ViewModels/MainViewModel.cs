using CommunityToolkit.Mvvm.ComponentModel;

namespace EquipmentControlCenter.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";
}

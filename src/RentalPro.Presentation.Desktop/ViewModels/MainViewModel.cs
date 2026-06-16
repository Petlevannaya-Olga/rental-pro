using CommunityToolkit.Mvvm.ComponentModel;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _currentPageTitle = "Дашборд";

    [ObservableProperty]
    private string _selectedMenuItem = "Дашборд";

    partial void OnSelectedMenuItemChanged(string value)
    {
        CurrentPageTitle = value;
    }
}
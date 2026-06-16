using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _currentPageTitle = "Дашборд";

    [RelayCommand]
    private void Navigate(string pageTitle)
    {
        CurrentPageTitle = pageTitle;
    }
}
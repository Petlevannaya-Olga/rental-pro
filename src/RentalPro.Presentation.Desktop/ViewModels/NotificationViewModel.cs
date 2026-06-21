using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class NotificationViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isOpen;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string message = string.Empty;

    [ObservableProperty]
    private string accentColor = "#1E73FF";

    [ObservableProperty]
    private PackIconKind icon = PackIconKind.InformationOutline;
    
    public double Opacity => IsOpen ? 1 : 0;

    public double Height => IsOpen ? 88 : 0;

    partial void OnIsOpenChanged(bool value)
    {
        OnPropertyChanged(nameof(Opacity));
        OnPropertyChanged(nameof(Height));
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Presentation.Desktop.Services;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class ReportsViewModel(
    NavigationService navigationService,
    NotificationService notificationService)
    : ObservableObject
{
    [RelayCommand]
    private void OpenReport(string report)
    {
        // Пока заглушка. Потом заменим на реальные страницы отчетов.
        notificationService.Info($"Открытие отчета: {report}");
    }
}
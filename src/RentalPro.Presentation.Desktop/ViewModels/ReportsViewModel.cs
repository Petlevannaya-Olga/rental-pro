using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class ReportsViewModel(
    NavigationService navigationService,
    NotificationService notificationService)
    : ObservableObject
{
    [ObservableProperty]
    private bool _isPeriodDialogOpen;

    [ObservableProperty]
    private string _selectedReportKey = string.Empty;

    [ObservableProperty]
    private string _selectedReportTitle = string.Empty;

    [ObservableProperty]
    private DateTime? _dateFrom = new DateTime(
        DateTime.Today.Year,
        DateTime.Today.Month,
        1);

    [ObservableProperty]
    private DateTime? _dateTo = DateTime.Today;

    [RelayCommand]
    private void OpenReport(string report)
    {
        SelectedReportKey = report;

        SelectedReportTitle = report switch
        {
            "revenue" => "Выручка по периодам",
            "popularTools" => "Популярность инструментов",
            "overdueReturns" => "Просроченные возвраты",
            "tools" => "Отчет по инструментам",
            "customers" => "Отчет по клиентам",
            "payments" => "Платежи по периодам",
            _ => "Отчет"
        };

        DateFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        DateTo = DateTime.Today;

        IsPeriodDialogOpen = true;
    }

    [RelayCommand]
    private void CancelPeriodDialog()
    {
        IsPeriodDialogOpen = false;
    }

    [RelayCommand]
    private async Task GenerateReportAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedReportKey))
        {
            notificationService.Error("Не выбран отчет");
            return;
        }

        if (DateFrom is null || DateTo is null)
        {
            notificationService.Error("Выберите период отчета");
            return;
        }

        if (DateFrom > DateTo)
        {
            notificationService.Error("Дата начала не может быть позже даты окончания");
            return;
        }

        IsPeriodDialogOpen = false;

        navigationService.NavigateTo<ReportResultView>(SelectedReportTitle);

        if (navigationService.CurrentView is not ReportResultView view ||
            view.DataContext is not ReportResultViewModel viewModel)
        {
            notificationService.Error("Не удалось открыть страницу отчета");
            return;
        }

        await viewModel.LoadAsync(new ReportPageParameters
        {
            ReportKey = SelectedReportKey,
            Title = SelectedReportTitle,
            DateFrom = DateFrom.Value,
            DateTo = DateTo.Value
        });
    }
}
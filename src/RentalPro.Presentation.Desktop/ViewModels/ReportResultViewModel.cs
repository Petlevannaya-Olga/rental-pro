using System.Data;
using System.IO;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class ReportResultViewModel(
    ReportsApiClient reportsApiClient,
    NavigationService navigationService,
    NotificationService notificationService)
    : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private DateTime _dateFrom;

    [ObservableProperty]
    private DateTime _dateTo;

    [ObservableProperty]
    private DataView? _items;

    [ObservableProperty]
    private bool _isLoading;

    private string _reportKey = string.Empty;

    public string PeriodText =>
        $"{DateFrom:dd.MM.yyyy} — {DateTo:dd.MM.yyyy}";

    public async Task LoadAsync(ReportPageParameters parameters)
    {
        _reportKey = parameters.ReportKey;

        Title = parameters.Title;
        DateFrom = parameters.DateFrom;
        DateTo = parameters.DateTo;

        OnPropertyChanged(nameof(PeriodText));

        await LoadReportAsync();
    }

    [RelayCommand]
    private void Back()
    {
        navigationService.NavigateTo<ReportsView>("Отчеты");
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Экспорт отчета",
            FileName = GetExportFileName(),
            Filter = "Excel файл (*.xlsx)|*.xlsx"
        };

        if (dialog.ShowDialog() != true)
            return;

        var result = _reportKey switch
        {
            "revenue" => await reportsApiClient.ExportRevenueAsync(DateFrom, DateTo),
            "popularTools" => await reportsApiClient.ExportPopularToolsAsync(DateFrom, DateTo),
            "overdueReturns" => await reportsApiClient.ExportOverdueReturnsAsync(),
            "tools" => await reportsApiClient.ExportToolsAsync(),
            "customers" => await reportsApiClient.ExportCustomersAsync(DateFrom, DateTo),
            "payments" => await reportsApiClient.ExportPaymentsAsync(DateFrom, DateTo),

            _ => throw new InvalidOperationException("Unknown report type")
        };

        if (result.IsFailure)
        {
            notificationService.Error(result.Error.Message);
            return;
        }

        await File.WriteAllBytesAsync(dialog.FileName, result.Value);

        notificationService.Success("Отчет экспортирован");
    }

    private async Task LoadReportAsync()
    {
        IsLoading = true;

        switch (_reportKey)
        {
            case "revenue":
            {
                var result = await reportsApiClient.GetRevenueAsync(DateFrom, DateTo);
                SetResult(result.IsSuccess ? result.Value : [], result.IsFailure ? result.Error.Message : null);
                break;
            }

            case "popularTools":
            {
                var result = await reportsApiClient.GetPopularToolsAsync(DateFrom, DateTo);
                SetResult(result.IsSuccess ? result.Value : [], result.IsFailure ? result.Error.Message : null);
                break;
            }

            case "overdueReturns":
            {
                var result = await reportsApiClient.GetOverdueReturnsAsync();
                SetResult(result.IsSuccess ? result.Value : [], result.IsFailure ? result.Error.Message : null);
                break;
            }

            case "tools":
            {
                var result = await reportsApiClient.GetToolsAsync();
                SetResult(result.IsSuccess ? result.Value : [], result.IsFailure ? result.Error.Message : null);
                break;
            }

            case "customers":
            {
                var result = await reportsApiClient.GetCustomersAsync(DateFrom, DateTo);
                SetResult(result.IsSuccess ? result.Value : [], result.IsFailure ? result.Error.Message : null);
                break;
            }

            case "payments":
            {
                var result = await reportsApiClient.GetPaymentsAsync(DateFrom, DateTo);
                SetResult(result.IsSuccess ? result.Value : [], result.IsFailure ? result.Error.Message : null);
                break;
            }

            default:
            {
                Items = null;
                IsLoading = false;
                notificationService.Error("Неизвестный тип отчета");
                break;
            }
        }
    }

    private void SetResult<T>(
        IReadOnlyList<T> rows,
        string? errorMessage)
    {
        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            Items = null;
            IsLoading = false;
            notificationService.Error(errorMessage);
            return;
        }

        Items = ToDataTable(rows).DefaultView;
        IsLoading = false;
    }

    private static DataTable ToDataTable<T>(IReadOnlyList<T> rows)
    {
        var table = new DataTable();

        var properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x =>
                x.CanRead &&
                !x.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var property in properties)
        {
            var type = Nullable.GetUnderlyingType(property.PropertyType)
                       ?? property.PropertyType;

            table.Columns.Add(GetColumnTitle(property.Name), type);
        }

        foreach (var row in rows)
        {
            var values = properties
                .Select(property => property.GetValue(row) ?? DBNull.Value)
                .ToArray();

            table.Rows.Add(values);
        }

        return table;
    }

    private static string GetColumnTitle(string propertyName)
    {
        return propertyName switch
        {
            "Period" => "Период",
            "Date" => "Дата",
            "OrderNumber" => "Номер заказа",
            "CustomerFullName" => "Клиент",
            "ToolName" => "Инструмент",
            "CategoryName" => "Категория",
            "ManufacturerName" => "Производитель",
            "StatusName" => "Статус",
            "PaymentTypeName" => "Тип оплаты",
            "PaymentMethodName" => "Способ оплаты",
            "OrdersCount" => "Количество заказов",
            "RentalsCount" => "Количество аренд",
            "PaymentsCount" => "Количество оплат",
            "Amount" => "Сумма",
            "TotalAmount" => "Сумма",
            "RevenueAmount" => "Выручка",
            "RentalAmount" => "Аренда",
            "DepositAmount" => "Залог",
            "DepositRefundAmount" => "Возврат залога",
            "PlannedReturnDate" => "Плановая дата возврата",
            "DaysOverdue" => "Дней просрочки",
            "ToolId" => "Id",
            "ToolsNames" => "Список инструментов",
            "OverdueDays" => "Количество дней просрочки",
            "ArticleNumber" => "Артикул",
            "RentalPricePerDay" => "Аренда в день",
            "RentAmount" => "Сумма аренды",
            "LastOrderDate" => "Дата последнего заказа",
            "PaymentType" => "Тип платежа",
            _ => propertyName
        };
    }

    private string GetExportFileName()
    {
        return _reportKey switch
        {
            "revenue" => "revenue-report.xlsx",
            "popularTools" => "popular-tools-report.xlsx",
            "overdueReturns" => "overdue-returns-report.xlsx",
            "tools" => "tools-report.xlsx",
            "customers" => "customers-report.xlsx",
            "payments" => "payments-report.xlsx",
            _ => "report.xlsx"
        };
    }
}
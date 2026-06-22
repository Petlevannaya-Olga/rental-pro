using CommunityToolkit.Mvvm.ComponentModel;

namespace RentalPro.Presentation.Desktop.Models;

public partial class OrderToolEditModel : ObservableObject
{
    [ObservableProperty]
    private Guid toolId;

    [ObservableProperty]
    private string toolName = string.Empty;

    [ObservableProperty]
    private decimal rentalPricePerDay;

    [ObservableProperty]
    private decimal depositAmount;

    [ObservableProperty]
    private DateTime startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime endDate = DateTime.Today.AddDays(3);

    public int RentalDays
    {
        get
        {
            var days = (EndDate.Date - StartDate.Date).Days;

            return days <= 0 ? 1 : days;
        }
    }

    public decimal TotalAmount =>
        RentalPricePerDay * RentalDays;

    partial void OnStartDateChanged(DateTime value)
    {
        OnPropertyChanged(nameof(RentalDays));
        OnPropertyChanged(nameof(TotalAmount));
    }

    partial void OnEndDateChanged(DateTime value)
    {
        OnPropertyChanged(nameof(RentalDays));
        OnPropertyChanged(nameof(TotalAmount));
    }
}
using CommunityToolkit.Mvvm.ComponentModel;

namespace RentalPro.Presentation.Desktop.Models;

public partial class OrderToolEditModel : ObservableObject
{
    [ObservableProperty] private Guid toolId;

    [ObservableProperty] private string toolName = string.Empty;

    [ObservableProperty] private decimal rentalPricePerDay;

    [ObservableProperty] private decimal depositAmount;

    [ObservableProperty] private DateTime startDate = DateTime.Today;

    [ObservableProperty] private DateTime endDate = DateTime.Today.AddDays(3);

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
        if (EndDate <= value)
            EndDate = value.AddDays(1);

        RefreshCalculatedFields();
    }

    partial void OnEndDateChanged(DateTime value)
    {
        if (value <= StartDate)
        {
            EndDate = StartDate.AddDays(1);
            return;
        }

        RefreshCalculatedFields();
    }

    private void RefreshCalculatedFields()
    {
        OnPropertyChanged(nameof(RentalDays));
        OnPropertyChanged(nameof(TotalAmount));
    }
}
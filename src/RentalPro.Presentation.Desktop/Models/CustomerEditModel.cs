using CommunityToolkit.Mvvm.ComponentModel;

namespace RentalPro.Presentation.Desktop.Models;

public partial class CustomerEditModel : ObservableObject
{
    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string middleName = string.Empty;

    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string passportSeries = string.Empty;

    [ObservableProperty]
    private string passportNumber = string.Empty;

    [ObservableProperty]
    private string postalCode = string.Empty;

    [ObservableProperty]
    private string region = string.Empty;

    [ObservableProperty]
    private string city = string.Empty;

    [ObservableProperty]
    private string street = string.Empty;

    [ObservableProperty]
    private string house = string.Empty;

    [ObservableProperty]
    private string? building;

    [ObservableProperty]
    private string? apartment;
}
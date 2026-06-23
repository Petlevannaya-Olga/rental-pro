using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.Customers;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Models;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Shared;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class CreateCustomerDialogViewModel(
    CustomersApiClient customersApiClient,
    FakeCustomerGeneratorService fakeCustomerGeneratorService,
    NotificationService notificationService)
    : ObservableObject
{
    [ObservableProperty]
    private CustomerEditModel customer = new();

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isSaving;

    public CreateCustomerResponse? CreatedCustomer { get; private set; }

    private bool IsFormFilled =>
        !string.IsNullOrWhiteSpace(Customer.LastName)
        && !string.IsNullOrWhiteSpace(Customer.FirstName)
        && !string.IsNullOrWhiteSpace(Customer.MiddleName)
        && !string.IsNullOrWhiteSpace(Customer.PhoneNumber)
        && !string.IsNullOrWhiteSpace(Customer.Email)
        && !string.IsNullOrWhiteSpace(Customer.PassportSeries)
        && !string.IsNullOrWhiteSpace(Customer.PassportNumber)
        && !string.IsNullOrWhiteSpace(Customer.PostalCode)
        && !string.IsNullOrWhiteSpace(Customer.Region)
        && !string.IsNullOrWhiteSpace(Customer.City)
        && !string.IsNullOrWhiteSpace(Customer.Street)
        && !string.IsNullOrWhiteSpace(Customer.House);

    private bool CanSave() =>
        !IsSaving && IsFormFilled;

    public void Open()
    {
        Customer = new CustomerEditModel();
        CreatedCustomer = null;
        ErrorMessage = string.Empty;
        IsSaving = false;

        Customer.PropertyChanged += (_, _) =>
        {
            SaveCommand.NotifyCanExecuteChanged();
        };

        SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task SaveAsync()
    {
        try
        {
            IsSaving = true;
            ErrorMessage = string.Empty;

            var request = new CreateCustomerRequest(
                Customer.LastName.Trim(),
                Customer.FirstName.Trim(),
                Customer.MiddleName.Trim(),
                Customer.PhoneNumber.Trim(),
                Customer.Email.Trim(),
                Customer.PassportSeries.Trim(),
                Customer.PassportNumber.Trim(),
                Customer.PostalCode.Trim(),
                Customer.Region.Trim(),
                Customer.City.Trim(),
                Customer.Street.Trim(),
                Customer.House.Trim(),
                NormalizeOptional(Customer.Building),
                NormalizeOptional(Customer.Apartment));

            var result = await customersApiClient.CreateCustomerAsync(request);

            if (result.IsFailure)
            {
                ErrorMessage = result.Error.Message;
                notificationService.Error(result.Error.Message);
                return;
            }

            CreatedCustomer = result.Value;
            notificationService.Success("Клиент добавлен");

            OnPropertyChanged(nameof(CreatedCustomer));
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void FillTestData()
    {
        Customer = fakeCustomerGeneratorService.Generate();

        Customer.PropertyChanged += (_, _) =>
        {
            SaveCommand.NotifyCanExecuteChanged();
        };

        ErrorMessage = string.Empty;
        SaveCommand.NotifyCanExecuteChanged();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    partial void OnIsSavingChanged(bool value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }
}
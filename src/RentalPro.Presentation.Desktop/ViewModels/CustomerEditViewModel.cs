using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Contracts.Customers;
using RentalPro.Presentation.Desktop.Api;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class CustomerEditViewModel(
    CustomersApiClient customersApiClient)
    : ObservableObject
{
    [ObservableProperty]
    private CustomerEditModel customer = new();

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private Guid? customerId;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public string Title =>
        IsEditMode ? "Редактирование клиента" : "Добавление клиента";

    public string SaveButtonText =>
        IsEditMode ? "Сохранить изменения" : "Сохранить";

    public void OpenCreate()
    {
        IsEditMode = false;
        CustomerId = null;
        Customer = new CustomerEditModel();
        ErrorMessage = string.Empty;

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    public void OpenEdit(CustomerDto dto)
    {
        IsEditMode = true;
        CustomerId = dto.Id;
        ErrorMessage = string.Empty;

        Customer = new CustomerEditModel
        {
            LastName = dto.LastName,
            FirstName = dto.FirstName,
            MiddleName = dto.MiddleName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            PassportSeries = dto.PassportSeries,
            PassportNumber = dto.PassportNumber,
            PostalCode = dto.PostalCode,
            Region = dto.Region,
            City = dto.City,
            Street = dto.Street,
            House = dto.House,
            Building = dto.Building,
            Apartment = dto.Apartment
        };

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (IsEditMode)
                await UpdateAsync();
            else
                await CreateAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private async Task CreateAsync()
    {
        var request = new CreateCustomerRequest(
            Customer.LastName,
            Customer.FirstName,
            Customer.MiddleName,
            Customer.PhoneNumber,
            Customer.Email,
            Customer.PassportSeries,
            Customer.PassportNumber,
            Customer.PostalCode,
            Customer.Region,
            Customer.City,
            Customer.Street,
            Customer.House,
            Customer.Building,
            Customer.Apartment);

        await customersApiClient.CreateCustomerAsync(request);
    }

    private async Task UpdateAsync()
    {
        if (CustomerId is null)
            return;

        var request = new UpdateCustomerRequest(
            Customer.LastName,
            Customer.FirstName,
            Customer.MiddleName,
            Customer.PhoneNumber,
            Customer.Email,
            Customer.PassportSeries,
            Customer.PassportNumber,
            Customer.PostalCode,
            Customer.Region,
            Customer.City,
            Customer.Street,
            Customer.House,
            Customer.Building,
            Customer.Apartment);

        await customersApiClient.UpdateCustomerAsync(CustomerId.Value, request);
    }
}
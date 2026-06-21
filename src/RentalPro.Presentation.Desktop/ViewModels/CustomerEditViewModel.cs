using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Contracts.Customers;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class CustomerEditViewModel(
    CustomersApiClient customersApiClient,
    NavigationService navigationService,
    FakeCustomerGeneratorService fakeCustomerGeneratorService)
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

    [ObservableProperty]
    private bool isSaving;

    public string Title =>
        IsEditMode
            ? "Редактирование клиента"
            : "Добавление клиента";

    public string SaveButtonText =>
        IsEditMode
            ? "Сохранить изменения"
            : "Сохранить клиента";

    public void OpenCreate()
    {
        IsEditMode = false;
        CustomerId = null;
        Customer = new CustomerEditModel();
        ErrorMessage = string.Empty;
        IsSaving = false;

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(SaveButtonText));

        SaveCommand.NotifyCanExecuteChanged();
    }

    public void OpenEdit(CustomerDto dto)
    {
        IsEditMode = true;
        CustomerId = dto.Id;
        ErrorMessage = string.Empty;
        IsSaving = false;

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

        SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void Back()
    {
        navigationService.NavigateTo<CustomersView>("Клиенты");
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        try
        {
            IsSaving = true;
            ErrorMessage = string.Empty;

            if (IsEditMode)
                await UpdateAsync();
            else
                await CreateAsync();

            navigationService.NavigateTo<CustomersView>("Клиенты");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsSaving = false;
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
    
    [RelayCommand]
    private void FillTestData()
    {
        Customer = fakeCustomerGeneratorService.Generate();
        ErrorMessage = string.Empty;
    }

    private bool CanSave()
    {
        return !IsSaving;
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
            throw new InvalidOperationException("Не выбран клиент для редактирования");

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

    partial void OnIsSavingChanged(bool value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }
}
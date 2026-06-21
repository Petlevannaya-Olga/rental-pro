using System.ComponentModel;
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
    FakeCustomerGeneratorService fakeCustomerGeneratorService,
    NotificationService notificationService)
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

    private bool CanSave()
    {
        return !IsSaving && IsFormFilled;
    }

    public void OpenCreate()
    {
        IsEditMode = false;
        CustomerId = null;
        ErrorMessage = string.Empty;
        IsSaving = false;

        SetCustomer(new CustomerEditModel());
        RefreshHeader();
    }

    public void OpenEdit(CustomerDto dto)
    {
        IsEditMode = true;
        CustomerId = dto.Id;
        ErrorMessage = string.Empty;
        IsSaving = false;

        SetCustomer(new CustomerEditModel
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
        });

        RefreshHeader();
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

            notificationService.Success(
                IsEditMode
                    ? "Данные клиента обновлены"
                    : "Клиент добавлен");
            
            navigationService.NavigateTo<CustomersView>("Клиенты");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void FillTestData()
    {
        SetCustomer(fakeCustomerGeneratorService.Generate());
        ErrorMessage = string.Empty;
        notificationService.Success("Сгенерированы тестовые данные");
    }

    private void SetCustomer(CustomerEditModel newCustomer)
    {
        Customer.PropertyChanged -= CustomerOnPropertyChanged;

        Customer = newCustomer;

        Customer.PropertyChanged += CustomerOnPropertyChanged;

        RefreshSaveState();
    }

    private void CustomerOnPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        RefreshSaveState();
    }

    private void RefreshHeader()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    private void RefreshSaveState()
    {
        SaveCommand.NotifyCanExecuteChanged();
    }

    private async Task CreateAsync()
    {
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

        await customersApiClient.CreateCustomerAsync(request);
    }

    private async Task UpdateAsync()
    {
        if (CustomerId is null)
            throw new InvalidOperationException("Не выбран клиент для редактирования");

        var request = new UpdateCustomerRequest(
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

        await customersApiClient.UpdateCustomerAsync(CustomerId.Value, request);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    partial void OnIsSavingChanged(bool value)
    {
        RefreshSaveState();
    }
}
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Contracts.Customers;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Common;
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
    private FormMode mode = FormMode.Create;

    [ObservableProperty]
    private Guid? customerId;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isSaving;
    
    
    public bool IsCreateMode => Mode == FormMode.Create;

    public bool IsEditMode => Mode == FormMode.Edit;

    public bool IsViewMode => Mode == FormMode.View;

    public bool IsEditable => Mode is FormMode.Create or FormMode.Edit;
    
    public bool CanShowTestDataButton => Mode == FormMode.Create;

    public string Title =>
        Mode switch
        {
            FormMode.Create => "Добавление клиента",
            FormMode.Edit => "Редактирование клиента",
            FormMode.View => "Просмотр клиента",
            _ => "Клиент"
        };

    public string SaveButtonText =>
        Mode == FormMode.Edit
            ? "Сохранить"
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
        return IsEditable
               && !IsSaving
               && IsFormFilled;
    }

    public void OpenCreate()
    {
        Mode = FormMode.Create;
        CustomerId = null;
        ErrorMessage = string.Empty;
        IsSaving = false;

        SetCustomer(new CustomerEditModel());
        RefreshState();
    }

    public void OpenEdit(CustomerDto dto)
    {
        Mode = FormMode.Edit;
        CustomerId = dto.Id;
        ErrorMessage = string.Empty;
        IsSaving = false;

        SetCustomerFromDto(dto);
        RefreshState();
    }

    public void OpenView(CustomerDto dto)
    {
        Mode = FormMode.View;
        CustomerId = dto.Id;
        ErrorMessage = string.Empty;
        IsSaving = false;

        SetCustomerFromDto(dto);
        RefreshState();
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

            if (Mode == FormMode.Edit)
                await UpdateAsync();
            else if (Mode == FormMode.Create)
                await CreateAsync();
            else
                return;

            notificationService.Success(
                Mode == FormMode.Edit
                    ? "Данные клиента обновлены"
                    : "Клиент добавлен");

            navigationService.NavigateTo<CustomersView>("Клиенты");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            notificationService.Error("Не удалось сохранить клиента");
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanFillTestData))]
    private void FillTestData()
    {
        SetCustomer(fakeCustomerGeneratorService.Generate());
        ErrorMessage = string.Empty;

        notificationService.Success("Сгенерированы тестовые данные");
    }

    private bool CanFillTestData()
    {
        return Mode == FormMode.Create && !IsSaving;
    }

    private void SetCustomerFromDto(CustomerDto dto)
    {
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

    private void RefreshState()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(SaveButtonText));
        OnPropertyChanged(nameof(IsCreateMode));
        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(IsViewMode));
        OnPropertyChanged(nameof(IsEditable));
        OnPropertyChanged(nameof(CanShowTestDataButton));

        RefreshSaveState();
        FillTestDataCommand.NotifyCanExecuteChanged();
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

    partial void OnModeChanged(FormMode value)
    {
        RefreshState();
    }

    partial void OnIsSavingChanged(bool value)
    {
        RefreshSaveState();
        FillTestDataCommand.NotifyCanExecuteChanged();
    }
}
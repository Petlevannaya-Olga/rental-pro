using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class DictionariesViewModel(
    DictionariesApiClient dictionariesApiClient,
    NavigationService navigationService,
    NotificationService notificationService)
    : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _toolCategoriesCount;

    [ObservableProperty]
    private int _toolStatusesCount;

    [ObservableProperty]
    private int _orderStatusesCount;

    [ObservableProperty]
    private int _paymentMethodsCount;

    [ObservableProperty]
    private int _paymentTypesCount;

    [ObservableProperty]
    private int _rolesCount;

    [ObservableProperty]
    private int _manufacturersCount;

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;

        var result = await dictionariesApiClient.GetStatsAsync();

        if (result.IsFailure)
        {
            IsLoading = false;
            notificationService.Error(result.Error.Message);
            return;
        }

        ToolCategoriesCount = result.Value.ToolCategories;
        ToolStatusesCount = result.Value.ToolStatuses;
        OrderStatusesCount = result.Value.OrderStatuses;
        PaymentMethodsCount = result.Value.PaymentMethods;
        PaymentTypesCount = result.Value.PaymentTypes;
        RolesCount = result.Value.Roles;
        ManufacturersCount = result.Value.Manufacturers;

        IsLoading = false;
    }

    [RelayCommand]
    private async Task OpenDictionaryAsync(string dictionary)
    {
        var parameters = dictionary switch
        {
            "toolCategories" => new DictionaryPageParameters
            {
                Title = "Категории инструментов",
                Description = "Категории и группы инструментов",
                Url = "api/tool-categories"
            },

            "toolStatuses" => new DictionaryPageParameters
            {
                Title = "Статусы инструментов",
                Description = "Статусы доступности инструментов",
                Url = "api/tool-statuses"
            },

            "orderStatuses" => new DictionaryPageParameters
            {
                Title = "Статусы заказов",
                Description = "Статусы выполнения заказов",
                Url = "api/order-statuses"
            },

            "paymentMethods" => new DictionaryPageParameters
            {
                Title = "Способы оплаты",
                Description = "Доступные способы оплаты",
                Url = "api/payment-methods"
            },

            "paymentTypes" => new DictionaryPageParameters
            {
                Title = "Типы оплат",
                Description = "Типы оплат и наценок",
                Url = "api/payment-types"
            },

            "roles" => new DictionaryPageParameters
            {
                Title = "Роли пользователей",
                Description = "Роли и права доступа",
                Url = "api/roles"
            },

            "manufacturers" => new DictionaryPageParameters
            {
                Title = "Производители",
                Description = "Производители инструментов",
                Url = "api/manufacturers"
            },

            _ => null
        };

        if (parameters is null)
            return;

        navigationService.NavigateTo<DictionaryDetailsView>("Справочники");

        if (navigationService.CurrentView is DictionaryDetailsView view &&
            view.DataContext is DictionaryDetailsViewModel viewModel)
        {
            await viewModel.LoadAsync(parameters);
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class DictionaryDetailsViewModel(
    DictionariesApiClient dictionariesApiClient,
    NavigationService navigationService,
    NotificationService notificationService)
    : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<DictionaryItemViewModel> _items = [];

    [ObservableProperty]
    private bool _isLoading;

    public async Task LoadAsync(DictionaryPageParameters parameters)
    {
        Title = parameters.Title;
        Description = parameters.Description;

        IsLoading = true;

        var result = await dictionariesApiClient.GetListAsync<DictionaryItemViewModel>(
            parameters.Url,
            "dictionary.items.load.failed",
            "Не удалось загрузить справочник");

        if (result.IsFailure)
        {
            Items = [];
            IsLoading = false;

            notificationService.Error(result.Error.Message);
            return;
        }

        Items = result.Value;
        IsLoading = false;
    }

    [RelayCommand]
    private void Back()
    {
        navigationService.NavigateTo<DictionariesView>("Справочники");
    }
}
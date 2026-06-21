using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Contracts.Tools;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class ToolRentalHistoryViewModel(
    ToolsApiClient toolsApiClient,
    NavigationService navigationService,
    NotificationService notificationService)
    : ObservableObject
{
    private Guid _toolId;

    [ObservableProperty]
    private string _toolName = string.Empty;

    [ObservableProperty]
    private string _inventoryNumber = string.Empty;
    
    [ObservableProperty]
    private string _serialNumber = string.Empty;

    [ObservableProperty]
    private List<ToolRentalHistoryItemDto> _items = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public async Task OpenAsync(ToolDto tool)
    {
        _toolId = tool.Id;

        ToolName = tool.Name;
        InventoryNumber = tool.InventoryNumber;
        SerialNumber = tool.SerialNumber;

        await LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var result = await toolsApiClient.GetRentalHistoryAsync(_toolId);

            if (result.IsFailure)
            {
                Items = [];
                ErrorMessage = result.Error.Message;
                notificationService.Error(result.Error.Message);
                return;
            }

            Items = result.Value;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Back()
    {
        navigationService.NavigateTo<ToolsView>("Инструменты");
    }
}
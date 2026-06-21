using CommunityToolkit.Mvvm.ComponentModel;

namespace RentalPro.Presentation.Desktop.Models;

public partial class ToolEditModel : ObservableObject
{
    [ObservableProperty]
    private string _articleNumber = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private Guid? _categoryId;

    [ObservableProperty]
    private Guid? _manufacturerId;

    [ObservableProperty]
    private Guid? _statusId;

    [ObservableProperty]
    private decimal _rentalPricePerDay;

    [ObservableProperty]
    private decimal _depositAmount;

    [ObservableProperty]
    private string _serialNumber = string.Empty;

    [ObservableProperty]
    private string _inventoryNumber = string.Empty;

    [ObservableProperty]
    private string? _currentCondition;

    [ObservableProperty]
    private string? _photoPath;
}
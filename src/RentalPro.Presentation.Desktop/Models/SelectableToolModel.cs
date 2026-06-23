using CommunityToolkit.Mvvm.ComponentModel;
using RentalPro.Contracts.Tools;

namespace RentalPro.Presentation.Desktop.Models;

public partial class SelectableToolModel : ObservableObject
{
    public SelectableToolModel(
        ToolDto tool,
        bool isSelected)
    {
        Tool = tool;
        IsSelected = isSelected;
    }

    public ToolDto Tool { get; }

    [ObservableProperty]
    private bool isSelected;

    public Guid Id => Tool.Id;

    public string Name => Tool.Name;

    public string CategoryName => Tool.CategoryName;

    public string ManufacturerName => Tool.ManufacturerName;

    public string SerialNumber => Tool.SerialNumber;

    public string InventoryNumber => Tool.InventoryNumber;

    public decimal RentalPricePerDay => Tool.RentalPricePerDay;

    public decimal DepositAmount => Tool.DepositAmount;

    public string StatusName => Tool.StatusName;
}
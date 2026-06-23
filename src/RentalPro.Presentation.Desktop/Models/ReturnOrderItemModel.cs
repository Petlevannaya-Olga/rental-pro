using CommunityToolkit.Mvvm.ComponentModel;

namespace RentalPro.Presentation.Desktop.Models;

public partial class ReturnOrderItemModel : ObservableObject
{
    public Guid Id { get; init; }

    public string ToolName { get; init; } = string.Empty;

    public DateOnly PlannedReturnDate { get; init; }

    [ObservableProperty]
    private bool isSelected;
}
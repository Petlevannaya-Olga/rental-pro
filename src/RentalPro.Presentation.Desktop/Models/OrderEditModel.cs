using CommunityToolkit.Mvvm.ComponentModel;

namespace RentalPro.Presentation.Desktop.Models;

public partial class OrderEditModel : ObservableObject
{
    [ObservableProperty]
    private Guid? customerId;

    [ObservableProperty]
    private string customerName = string.Empty;

    [ObservableProperty]
    private DateTime orderDate = DateTime.Today;

    [ObservableProperty]
    private string? comment;

    [ObservableProperty]
    private List<OrderToolEditModel> tools = [];
}
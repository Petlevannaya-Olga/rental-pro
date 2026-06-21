using System.Windows.Controls;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class CustomerOrderHistoryView : UserControl
{
    public CustomerOrderHistoryView(
        CustomerOrderHistoryViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}
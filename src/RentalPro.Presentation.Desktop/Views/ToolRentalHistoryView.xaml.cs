using System.Windows.Controls;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class ToolRentalHistoryView : UserControl
{
    public ToolRentalHistoryView(
        ToolRentalHistoryViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}
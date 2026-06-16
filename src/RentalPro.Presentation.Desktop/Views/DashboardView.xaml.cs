using System.Windows.Controls;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class DashboardView : UserControl
{
    public DashboardView(DashboardViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;

        Loaded += async (_, _) =>
        {
            await viewModel.LoadCommand.ExecuteAsync(null);
        };
    }
}
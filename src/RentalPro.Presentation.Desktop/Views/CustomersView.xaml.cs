using System.Windows.Controls;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class CustomersView : UserControl
{
    public CustomersView(CustomersViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;

        Loaded += async (_, _) =>
        {
            await viewModel.LoadCommand.ExecuteAsync(null);
        };
    }
}
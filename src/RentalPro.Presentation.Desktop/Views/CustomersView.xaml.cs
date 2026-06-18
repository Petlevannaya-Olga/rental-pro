using System.Windows.Controls;
using System.Windows.Input;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class CustomersView : UserControl
{
    private readonly CustomersViewModel _viewModel;

    public CustomersView(CustomersViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += async (_, _) =>
        {
            await _viewModel.LoadCommand.ExecuteAsync(null);
        };
    }

    private async void CustomersDataGrid_OnSorting(
        object sender,
        DataGridSortingEventArgs e)
    {
        e.Handled = true;

        if (e.Column.SortMemberPath is null)
            return;

        await _viewModel.SortCommand.ExecuteAsync(e.Column.SortMemberPath.ToLowerInvariant());
    }
}
using System.ComponentModel;
using System.Windows.Controls;
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

        if (string.IsNullOrWhiteSpace(e.Column.SortMemberPath))
            return;

        var dataGrid = (DataGrid)sender;
        var sortBy = e.Column.SortMemberPath;

        await _viewModel.SortCommand.ExecuteAsync(sortBy);

        foreach (var column in dataGrid.Columns)
            column.SortDirection = null;

        e.Column.SortDirection = _viewModel.Descending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;
    }
}
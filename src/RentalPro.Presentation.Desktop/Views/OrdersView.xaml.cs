using System.ComponentModel;
using System.Windows.Controls;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class OrdersView : UserControl
{
    private readonly OrdersViewModel _viewModel;

    public OrdersView(OrdersViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += async (_, _) =>
        {
            await _viewModel.LoadCommand.ExecuteAsync(null);
        };
    }

    private async void OrdersDataGrid_OnSorting(
        object sender,
        DataGridSortingEventArgs e)
    {
        e.Handled = true;

        if (e.Column.SortMemberPath is null)
            return;

        var dataGrid = (DataGrid)sender;

        foreach (var column in dataGrid.Columns)
            column.SortDirection = null;

        var sortBy = e.Column.SortMemberPath.ToLowerInvariant();

        e.Column.SortDirection =
            _viewModel.SortBy == sortBy && !_viewModel.Descending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;

        await _viewModel.SortCommand.ExecuteAsync(sortBy);
    }
}
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using PaymentsViewModel = RentalPro.Presentation.Desktop.ViewModels.PaymentsViewModel;

namespace RentalPro.Presentation.Desktop.Views;

public partial class PaymentsView : UserControl
{
    public PaymentsView(PaymentsViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;

        Loaded += PaymentsView_Loaded;
    }

    private async void PaymentsView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (DataContext is not PaymentsViewModel viewModel)
            return;

        await viewModel.LoadAsync();
    }

    private async void PaymentsDataGrid_OnSorting(
        object sender,
        DataGridSortingEventArgs e)
    {
        e.Handled = true;

        if (DataContext is not PaymentsViewModel viewModel)
            return;

        var sortBy = e.Column.SortMemberPath;

        if (string.IsNullOrWhiteSpace(sortBy))
            return;

        await viewModel.SortCommand.ExecuteAsync(sortBy);

        e.Column.SortDirection =
            viewModel.SortBy == sortBy
                ? (viewModel.Descending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending)
                : null;
    }
}
using System.Windows;
using RentalPro.Contracts.Orders;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class CloseRentalDialog : Window
{
    public CloseRentalDialogViewModel ViewModel { get; }

    public bool IsSaved =>
        ViewModel.IsSaved;

    public CloseRentalDialog(CloseRentalDialogViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        DataContext = ViewModel;

        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CloseRentalDialogViewModel.IsSaved)
                && ViewModel.IsSaved)
            {
                DialogResult = true;
                Close();
            }
        };
    }

    public async Task OpenAsync(OrderDetailsDto order)
    {
        await ViewModel.OpenAsync(order);
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
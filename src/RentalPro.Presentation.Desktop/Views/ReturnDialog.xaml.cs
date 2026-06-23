using System.Windows;
using RentalPro.Contracts.Orders;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class ReturnDialog : Window
{
    public ReturnDialogViewModel ViewModel { get; }

    public bool IsSaved =>
        ViewModel.IsSaved;

    public ReturnDialog(ReturnDialogViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        DataContext = ViewModel;

        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ReturnDialogViewModel.IsSaved)
                && ViewModel.IsSaved)
            {
                DialogResult = true;
                Close();
            }
        };
    }

    public void Open(OrderDetailsDto order)
    {
        ViewModel.Open(order);
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
using System.Windows;
using RentalPro.Contracts.Customers;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class CreateCustomerDialog : Window
{
    public CreateCustomerDialogViewModel ViewModel { get; }

    public CreateCustomerResponse? CreatedCustomer =>
        ViewModel.CreatedCustomer;

    public CreateCustomerDialog(CreateCustomerDialogViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        DataContext = ViewModel;

        Loaded += (_, _) =>
        {
            ViewModel.Open();
        };

        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CreateCustomerDialogViewModel.CreatedCustomer)
                && ViewModel.CreatedCustomer is not null
                && IsLoaded)
            {
                DialogResult = true;
                Close();
            }
        };
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
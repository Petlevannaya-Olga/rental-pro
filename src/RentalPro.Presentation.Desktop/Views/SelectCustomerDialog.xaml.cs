using System.Windows;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class SelectCustomerDialog : Window
{
    public SelectCustomerDialogViewModel ViewModel { get; }

    public SelectCustomerDialog(SelectCustomerDialogViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        DataContext = ViewModel;

        Loaded += async (_, _) =>
        {
            await ViewModel.LoadAsync();
        };
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Select_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.SelectCommand.Execute(null);

        if (ViewModel.Result is null)
            return;

        DialogResult = true;
        Close();
    }
}
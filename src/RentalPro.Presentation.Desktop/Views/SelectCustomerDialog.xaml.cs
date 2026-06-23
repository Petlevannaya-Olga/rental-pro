using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class SelectCustomerDialog : Window
{
    private readonly IServiceProvider _serviceProvider;

    public SelectCustomerDialogViewModel ViewModel { get; }

    public SelectCustomerDialog(
        SelectCustomerDialogViewModel viewModel,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _serviceProvider = serviceProvider;

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

        if (ViewModel.SelectedCustomerId is null)
            return;

        DialogResult = true;
        Close();
    }

    private async void CreateCustomer_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = _serviceProvider.GetRequiredService<CreateCustomerDialog>();
        dialog.Owner = this;

        dialog.ShowDialog();

        if (dialog.CreatedCustomer is null)
            return;

        await ViewModel.LoadAsync();

        ViewModel.SelectCreatedCustomer(
            dialog.CreatedCustomer.Id,
            dialog.CreatedCustomer.FullName);

        DialogResult = true;
        Close();
    }
}
using System.Windows;
using RentalPro.Presentation.Desktop.Models;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class SelectToolsDialog : Window
{
    public SelectToolsDialogViewModel ViewModel { get; }

    public SelectToolsDialog(SelectToolsDialogViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        DataContext = ViewModel;
    }

    public async Task LoadAsync(IEnumerable<OrderToolEditModel> selectedTools)
    {
        await ViewModel.LoadAsync(selectedTools);
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Select_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.ConfirmCommand.Execute(null);

        DialogResult = true;
        Close();
    }
}
using System.Windows;
using System.Windows.Controls;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class ReportsView : UserControl
{
    public ReportsView(ReportsViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;

        Loaded += ReportsView_Loaded;
    }

    private async void ReportsView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (DataContext is ReportsViewModel viewModel)
            await viewModel.LoadAsync();
    }
}
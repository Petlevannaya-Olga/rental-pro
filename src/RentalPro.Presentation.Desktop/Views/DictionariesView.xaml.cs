using System.Windows;
using System.Windows.Controls;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class DictionariesView : UserControl
{
    public DictionariesView(DictionariesViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;

        Loaded += DictionariesView_Loaded;
    }

    private async void DictionariesView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (DataContext is not DictionariesViewModel viewModel)
            return;

        await viewModel.LoadAsync();
    }
}
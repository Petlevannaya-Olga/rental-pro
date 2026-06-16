using System.Windows;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
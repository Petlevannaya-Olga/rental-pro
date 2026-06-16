using System.Windows;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class LoginWindow : Window
{
    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
using System.Windows.Controls;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class CustomerEditView : UserControl
{
    public CustomerEditView(CustomerEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
using System.Windows.Controls;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class OrderEditView : UserControl
{
    public OrderEditView(OrderEditViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}
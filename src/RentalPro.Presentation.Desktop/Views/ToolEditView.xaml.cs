using System.Windows.Controls;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class ToolEditView : UserControl
{
    public ToolEditView(ToolEditViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}
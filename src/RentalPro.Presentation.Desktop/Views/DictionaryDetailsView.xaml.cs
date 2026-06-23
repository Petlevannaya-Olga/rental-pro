using System.Windows.Controls;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class DictionaryDetailsView : UserControl
{
    public DictionaryDetailsView(DictionaryDetailsViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}
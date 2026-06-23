using System.Windows.Controls;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class ReportResultView : UserControl
{
    public ReportResultView(ReportResultViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}
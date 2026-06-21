using System.ComponentModel;
using System.Windows.Controls;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Views;

public partial class ToolsView : UserControl
{
    private readonly ToolsViewModel _viewModel;

    public ToolsView(ToolsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += async (_, _) =>
        {
            await _viewModel.LoadCommand.ExecuteAsync(null);
        };
    }
}
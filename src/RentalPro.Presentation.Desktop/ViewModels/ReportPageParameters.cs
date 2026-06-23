namespace RentalPro.Presentation.Desktop.ViewModels;

public sealed class ReportPageParameters
{
    public required string ReportKey { get; init; }

    public required string Title { get; init; }

    public required DateTime DateFrom { get; init; }

    public required DateTime DateTo { get; init; }
}
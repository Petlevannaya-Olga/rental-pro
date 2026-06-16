using RentalPro.Contracts.Dashboard;

namespace RentalPro.Presentation.Desktop.ViewModels;

public sealed class DashboardReturnItemViewModel
{
    public required Guid OrderId { get; init; }

    public required string CustomerFullName { get; init; }

    public required string ToolsNames { get; init; }

    public required DateOnly PlannedReturnDate { get; init; }

    public required int Days { get; init; }

    public required bool IsOverdue { get; init; }

    public string DateText =>
        IsOverdue
            ? $"{Math.Abs(Days)} дн. просрочки"
            : Days switch
            {
                0 => "Сегодня",
                1 => "Завтра",
                _ => PlannedReturnDate.ToString("dd.MM.yyyy")
            };

    public static DashboardReturnItemViewModel FromDto(
        DashboardReturnDto dto,
        bool isOverdue)
    {
        return new DashboardReturnItemViewModel
        {
            OrderId = dto.OrderId,
            CustomerFullName = dto.CustomerFullName,
            ToolsNames = dto.ToolsNames,
            PlannedReturnDate = dto.PlannedReturnDate,
            Days = dto.Days,
            IsOverdue = isOverdue
        };
    }
}
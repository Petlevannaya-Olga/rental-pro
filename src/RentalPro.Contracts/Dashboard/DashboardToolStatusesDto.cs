namespace RentalPro.Contracts.Dashboard;

public sealed record DashboardToolStatusesDto(
    int Available,
    int Rented,
    int Booked,
    int InRepair);
namespace RentalPro.Contracts.Customers;

public sealed record CustomerStatsDto(
    int TotalCount,
    int WithOrdersCount,
    int WithActiveOrdersCount);
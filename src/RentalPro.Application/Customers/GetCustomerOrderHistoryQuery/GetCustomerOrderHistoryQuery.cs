using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Customers.GetCustomerOrderHistoryQuery;

public sealed record GetCustomerOrderHistoryQuery(Guid CustomerId)  : IQuery;
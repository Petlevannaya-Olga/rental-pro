using CSharpFunctionalExtensions;
using RentalPro.Application.Extensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.PaymentMethods;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.PaymentMethods.GetPaymentMethodQuery;

public sealed class GetPaymentMethodsHandler(
    IPaymentMethodsRepository paymentMethodsRepository)
    : IQueryHandler<IReadOnlyList<PaymentMethodDto>, GetPaymentMethodsQuery>
{
    public async Task<Result<IReadOnlyList<PaymentMethodDto>, Errors>> Handle(
        GetPaymentMethodsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await paymentMethodsRepository.GetAllAsync(cancellationToken);

        if (result.IsFailure)
            return result.Error.ToErrors();

        return result.Value
            .Select(x => x.ToDto())
            .ToList();
    }
}
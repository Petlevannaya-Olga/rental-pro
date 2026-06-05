using CSharpFunctionalExtensions;
using RentalPro.Application.Extensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.PaymentTypes;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.PaymentTypes.GetPaymentTypeQuery;

public sealed class GetPaymentTypesHandler(
    IPaymentTypesRepository paymentTypesRepository)
    : IQueryHandler<IReadOnlyList<PaymentTypeDto>, GetPaymentTypesQuery>
{
    public async Task<Result<IReadOnlyList<PaymentTypeDto>, Errors>> Handle(
        GetPaymentTypesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await paymentTypesRepository.GetAllAsync(cancellationToken);

        if (result.IsFailure)
            return result.Error.ToErrors();

        return result.Value
            .Select(x => x.ToDto())
            .ToList();
    }
}
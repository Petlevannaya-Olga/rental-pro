using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Manufacturers;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Manufactures.UpdateManufactureCommand;

public sealed class UpdateManufacturerCommandHandler(
    IManufacturerRepository repository,
    ITransactionManager transactionManager,
    ILogger<UpdateManufacturerCommandHandler> logger)
    : ICommandHandler<UpdateManufacturerCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        UpdateManufacturerCommand command,
        CancellationToken cancellationToken)
    {
        var manufacturerId = ManufacturerId.Restore(command.Id);

        var manufacturerResult = await repository.GetByAsync(
            x => x.Id == manufacturerId,
            cancellationToken);

        if (manufacturerResult.IsFailure)
            return manufacturerResult.Error.ToErrors();

        if (manufacturerResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "manufacturer.not.found",
                    $"Manufacturer with id '{command.Id}' was not found")
                .ToErrors();
        }

        var updateResult = manufacturerResult.Value.Update(command.Name, command.Country);

        if (updateResult.IsFailure)
            return updateResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Manufacturer with id '{ManufacturerId}' was updated",
            command.Id);

        return UnitResult.Success<Errors>();
    }
}
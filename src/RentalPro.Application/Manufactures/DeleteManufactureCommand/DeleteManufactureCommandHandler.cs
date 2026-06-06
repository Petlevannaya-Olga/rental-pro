using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Manufacturers;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Manufactures.DeleteManufactureCommand;

public sealed class DeleteManufacturerCommandHandler(
    IManufacturerRepository repository,
    ITransactionManager transactionManager,
    ILogger<DeleteManufacturerCommandHandler> logger)
    : ICommandHandler<Manufactures.DeleteManufactureCommand.DeleteManufacturerCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        DeleteManufacturerCommand command,
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

        var deleteResult = manufacturerResult.Value.Delete();

        if (deleteResult.IsFailure)
            return deleteResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Manufacturer with id '{ManufacturerId}' was deleted",
            command.Id);

        return UnitResult.Success<Errors>();
    }
}
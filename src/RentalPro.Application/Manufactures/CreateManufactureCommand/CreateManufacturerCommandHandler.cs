using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Manufacturers;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Manufactures.CreateManufactureCommand;

public sealed class CreateManufacturerCommandHandler(
    IManufacturerRepository repository,
    ILogger<CreateManufacturerCommandHandler> logger)
    : ICommandHandler<CreateManufacturerCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        CreateManufacturerCommand command,
        CancellationToken cancellationToken)
    {
        var manufacturerResult = Manufacturer.Create(command.Name, command.Country);

        if (manufacturerResult.IsFailure)
            return manufacturerResult.Error.ToErrors();

        var addResult = await repository.AddAsync(
            manufacturerResult.Value,
            cancellationToken);

        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        logger.LogInformation(
            "Manufacturer '{ManufacturerName}' was created",
            command.Name);

        return UnitResult.Success<Errors>();
    }
}
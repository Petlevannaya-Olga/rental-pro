using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Decorators;

public class ValidationDecorator<TResponse, TCommand>(
    IEnumerable<IValidator<TCommand>> validators,
    ICommandHandler<TResponse, TCommand> inner,
    ILogger<ValidationDecorator<TResponse, TCommand>> logger)
    : ICommandHandler<TResponse, TCommand>
    where TCommand : IValidation
{
    public async Task<Result<TResponse, Errors>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await inner.Handle(command, cancellationToken);
        }

        var context = new ValidationContext<TCommand>(command);
        var validationResults = await Task.WhenAll(
            validators.Select(x => x.ValidateAsync(context, cancellationToken)));

        var results = validationResults
            .Where(x => !x.IsValid)
            .ToList();

        if (results.Count > 0)
        {
            var errors = results.ToErrors();
            logger.LogError("Ошибки валидации: {@Errors}", errors);
            return errors;
        }

        var result = await inner.Handle(command, cancellationToken);

        return result;
    }
}
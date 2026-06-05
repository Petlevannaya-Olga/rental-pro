namespace RentalPro.Shared;

public static class CommonErrors
{
    public static Error None = new(string.Empty, string.Empty, ErrorType.NONE);

    public static Error IsRequired(string propertyName) =>
        new(
            $"{propertyName.ToLowerInvariant()}.is.required",
            $"Значение не задано для {propertyName}",
            ErrorType.VALIDATION,
            propertyName);

    public static Error LengthIsWrong(string propertyName, int minLength, int maxLength)
        => new(
            $"{propertyName.ToLowerInvariant()}.length.is.wrong",
            $"Значение должно быть длиной от {minLength} до {maxLength} символов для {propertyName}",
            ErrorType.VALIDATION,
            propertyName);

    public static Error ValueIsGreaterThanMax(string propertyName, double value, double max)
        => new(
            $"{propertyName.ToLowerInvariant()}.length.is.greater.than.max",
            $"Значение {value} не должно превышать {max} для {propertyName}",
            ErrorType.VALIDATION,
            propertyName);

    public static Error ValueIsLessThanMin(string propertyName, double value, double min)
        => new(
            $"{propertyName.ToLowerInvariant()}.length.is.greater.than.min",
            $"Значение {value} не должно быть меньше {min} для {propertyName}",
            ErrorType.VALIDATION,
            propertyName);

    public static Error LengthIsTooShort(string propertyName, int minLength)
        => new(
            $"{propertyName.ToLowerInvariant()}.length.is.too.short",
            $"Значение должно быть не менее {minLength} символов",
            ErrorType.VALIDATION,
            propertyName);

    public static Error LengthIsTooLarge(string propertyName, int maxLength)
        => new(
            $"{propertyName.ToLowerInvariant()}.length.is.too.large",
            $"Значение должно быть не более {maxLength} символов",
            ErrorType.VALIDATION,
            propertyName);

    public static Error MustBePositive(string propertyName)
        => new(
            $"{propertyName.ToLowerInvariant()}.must.be.positive",
            $"{propertyName} должно быть положительно",
            ErrorType.VALIDATION,
            propertyName);

    public static Error NotFound(string? code, string message, Guid? id = null)
        => new(code ?? "record.not.found", message, ErrorType.NOT_FOUND);

    public static Error Validation(string? code, string message, string? invalidField = null)
        => new(code ?? "value.is.invalid", message, ErrorType.VALIDATION, invalidField);

    public static Error Conflict(string? code, string message)
        => new(code ?? "value.is.conflict", message, ErrorType.CONFLICT);

    public static Error Failure(string? code, string message)
        => new(code ?? "failure", message, ErrorType.FAILURE);

    public static Error Db(string? code, string message)
        => new(code ?? "db.exception", message, ErrorType.DB);

    public static Error OperationCancelled(string? code)
        => new(code ?? "operation.cancelled", "Операция была отменена", ErrorType.CANCELED);

    public static Error CollectionIsEmpty(string? code)
        => new(code ?? "array.is.empty", "Массив не может быть пустым", ErrorType.VALIDATION);

    public static Error CollectionContainsDublicates(string? code = null)
        => new(code ?? "collection.contains.dublicates", "Коллекция содержит дубликаты", ErrorType.VALIDATION);

    public static Error AuthFailed(string? code, string message) =>
        new(code ?? "auth.failed", message, ErrorType.FAILURE);

    public static Error EmptyResponse(string? code, string message) =>
        new(
            code ?? "empty.response",
            "Сервер вернул пустой ответ",
            ErrorType.FAILURE);

    public static Error LoadFailed(string? code, string message) =>
        new(code ?? "load.failed", message, ErrorType.FAILURE);

    public static Error CreateFailed(string? code, string message) =>
        new(code ?? "create.failed", message, ErrorType.FAILURE);
    
    public static Error UpdateFailed(string? code, string message) =>
        new(code ?? "update.failed", message, ErrorType.FAILURE);
    
    public static Error DeleteFailed(string? code, string message) =>
        new(code ?? "delete.failed", message, ErrorType.FAILURE);
}
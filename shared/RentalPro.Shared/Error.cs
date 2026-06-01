namespace RentalPro.Shared;

using System.Text.Json.Serialization;

[method: JsonConstructor]
public record Error(string Code, string Message, ErrorType Type, string? InvalidField = null)
{
    private const string SEPARATOR = "||";

    /// <summary>
    /// Код ошибки
    /// </summary>
    public string Code { get; } = Code;

    /// <summary>
    /// Текст ошибки
    /// </summary>
    public string Message { get; } = Message;

    /// <summary>
    /// Тип ошибки
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ErrorType Type { get; } = Type;

    /// <summary>
    /// Поле, в котором произошла ошибка
    /// </summary>
    public string? InvalidField { get; } = InvalidField;

    public Errors ToErrors() => this;

    public string Serialize()
    {
        return string.Join(SEPARATOR, Code, Message, Type);
    }

    public static Error Deserialize(string json)
    {
        var parts = json.Split(SEPARATOR);

        if (parts.Length != 3 || !Enum.TryParse(parts[2], out ErrorType errorType))
        {
            throw new ArgumentException("Invalid string format");
        }

        return new Error(parts[0], parts[1], errorType);
    }
}

public enum ErrorType
{
    /// <summary>
    /// Отсутствие ошибки
    /// </summary>
    NONE,

    /// <summary>
    /// Ошибка валидации
    /// </summary>
    VALIDATION,

    /// <summary>
    /// Ничего не найдено
    /// </summary>
    NOT_FOUND,

    /// <summary>
    /// Серверная ошибка
    /// </summary>
    FAILURE,

    /// <summary>
    /// Конфликт
    /// </summary>
    CONFLICT,

    /// <summary>
    /// Ошибка базы данных
    /// </summary>
    DB,
    
    /// <summary>
    /// Операция была отменена
    /// </summary>
    CANCELED,
}
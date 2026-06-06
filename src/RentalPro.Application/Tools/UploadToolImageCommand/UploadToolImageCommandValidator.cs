using FluentValidation;
using RentalPro.Domain.Tools;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Tools.UploadToolImageCommand;

public sealed class UploadToolImageCommandValidator
    : AbstractValidator<UploadToolImageCommand>
{
    private static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    private const long MaxFileSize = 10 * 1024 * 1024;

    public UploadToolImageCommandValidator()
    {
        RuleFor(x => x.ToolId)
            .MustBeValueObject(ToolId.Create);

        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("Файл изображения не передан");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("Имя файла не указано")
            .Must(HaveAllowedExtension)
            .WithMessage("Допустимые форматы изображения: jpg, jpeg, png, webp");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("Файл изображения пустой")
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage("Размер изображения не должен превышать 10 МБ");
    }

    private static bool HaveAllowedExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName);

        if (string.IsNullOrWhiteSpace(extension))
            return false;

        return AllowedExtensions.Contains(
            extension.ToLowerInvariant());
    }
}
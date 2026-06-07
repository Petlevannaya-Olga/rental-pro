using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Hosting;
using RentalPro.Application.Files;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Files;

public sealed class LocalFileStorage(
    IWebHostEnvironment environment)
    : IFileStorage
{
    private static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    public async Task<Result<string, Error>> SaveToolImageAsync(
        Stream stream,
        string originalFileName,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            return CommonErrors.Validation(
                "tool.image.invalid.extension",
                "Допустимые форматы изображения: jpg, jpeg, png, webp");
        }

        var webRootPath = environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
            webRootPath = Path.Combine(environment.ContentRootPath, "wwwroot");

        var uploadsFolder = Path.Combine(
            webRootPath,
            "uploads",
            "tools");

        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";

        var filePath = Path.Combine(
            uploadsFolder,
            fileName);

        await using var fileStream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write);

        await stream.CopyToAsync(
            fileStream,
            cancellationToken);

        return $"/uploads/tools/{fileName}";
    }
    
    public Task<UnitResult<Error>> DeleteToolImageAsync(
        string photoPath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(photoPath))
            return Task.FromResult(UnitResult.Success<Error>());

        try
        {
            var webRootPath = environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
                webRootPath = Path.Combine(environment.ContentRootPath, "wwwroot");

            var relativePath = photoPath.TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar);

            var filePath = Path.Combine(webRootPath, relativePath);

            if (File.Exists(filePath))
                File.Delete(filePath);

            return Task.FromResult(UnitResult.Success<Error>());
        }
        catch
        {
            return Task.FromResult(
                UnitResult.Failure(
                    CommonErrors.Failure(
                        "tool.image.delete.failed",
                        "Не удалось удалить старое фото инструмента")));
        }
    }
}
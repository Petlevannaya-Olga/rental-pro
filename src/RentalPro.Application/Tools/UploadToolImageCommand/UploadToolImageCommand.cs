using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.UploadToolImageCommand;

public sealed record UploadToolImageCommand(
    Guid ToolId,
    Stream FileStream,
    string FileName,
    long FileSize) : IValidation;
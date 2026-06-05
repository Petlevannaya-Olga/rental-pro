namespace RentalPro.Presentation.Client.Models;

public sealed record DictionarySettings(
    string Title,
    string Url,
    string CreateSuccessMessage,
    string UpdateSuccessMessage,
    string DeleteSuccessMessage,
    string LoadErrorCode,
    string CreateErrorCode,
    string UpdateErrorCode,
    string DeleteErrorCode,
    string LoadErrorMessage,
    string CreateErrorMessage,
    string UpdateErrorMessage,
    string DeleteErrorMessage);
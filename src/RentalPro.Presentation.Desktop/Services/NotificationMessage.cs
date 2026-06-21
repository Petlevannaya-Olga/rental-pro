using MaterialDesignThemes.Wpf;

namespace RentalPro.Presentation.Desktop.Services;

public sealed record NotificationMessage(
    string Text,
    NotificationType Type)
{
    public string AccentColor => Type switch
    {
        NotificationType.Success => "#16A34A",
        NotificationType.Info => "#1E73FF",
        NotificationType.Warning => "#F59E0B",
        NotificationType.Error => "#DC2626",
        _ => "#1E73FF"
    };

    public PackIconKind Icon => Type switch
    {
        NotificationType.Success => PackIconKind.CheckCircleOutline,
        NotificationType.Info => PackIconKind.InformationOutline,
        NotificationType.Warning => PackIconKind.AlertOutline,
        NotificationType.Error => PackIconKind.CloseCircleOutline,
        _ => PackIconKind.InformationOutline
    };

    public string Title => Type switch
    {
        NotificationType.Success => "Готово",
        NotificationType.Info => "Информация",
        NotificationType.Warning => "Внимание",
        NotificationType.Error => "Ошибка",
        _ => "Информация"
    };
}
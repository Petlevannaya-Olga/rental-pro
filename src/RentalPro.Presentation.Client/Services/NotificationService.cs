namespace RentalPro.Presentation.Client.Services;

public class NotificationService
{
    public event Action<NotificationMessage>? OnNotification;

    public void Success(string message)
    {
        OnNotification?.Invoke(
            new NotificationMessage(message, NotificationType.Success));
    }

    public void Error(string message)
    {
        OnNotification?.Invoke(
            new NotificationMessage(message, NotificationType.Error));
    }

    public void Warning(string message)
    {
        OnNotification?.Invoke(
            new NotificationMessage(message, NotificationType.Warning));
    }

    public void Info(string message)
    {
        OnNotification?.Invoke(
            new NotificationMessage(message, NotificationType.Info));
    }
}

public record NotificationMessage(
    string Text,
    NotificationType Type);

public enum NotificationType
{
    Success,
    Error,
    Warning,
    Info
}
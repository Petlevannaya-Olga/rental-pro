using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;

namespace RentalPro.Presentation.Desktop.Services;

public sealed class NotificationService
{
    public SnackbarMessageQueue MessageQueue { get; } = new();

    public void Success(string message)
    {
        MessageQueue.Enqueue(message);
    }

    public void Error(string message)
    {
        MessageQueue.Enqueue(message);
    }

    public void Info(string message)
    {
        MessageQueue.Enqueue(message);
    }
}
using System.Windows;
using MaterialDesignThemes.Wpf;
using RentalPro.Presentation.Desktop.ViewModels;

namespace RentalPro.Presentation.Desktop.Services;

public sealed class NotificationService(NotificationViewModel notification)
{
    private CancellationTokenSource? _closeCts;

    public NotificationViewModel Notification => notification;

    public void Success(string message)
    {
        Show("Готово", message, "#16A34A", PackIconKind.CheckCircleOutline);
    }

    public void Info(string message)
    {
        Show("Информация", message, "#1E73FF", PackIconKind.InformationOutline);
    }

    public void Warning(string message)
    {
        Show("Внимание", message, "#F59E0B", PackIconKind.AlertOutline);
    }

    public void Error(string message)
    {
        Show("Ошибка", message, "#DC2626", PackIconKind.CloseCircleOutline);
    }

    private void Show(
        string title,
        string message,
        string accentColor,
        PackIconKind icon)
    {
        _closeCts?.Cancel();
        _closeCts = new CancellationTokenSource();

        Application.Current.Dispatcher.Invoke(() =>
        {
            notification.Title = title;
            notification.Message = message;
            notification.AccentColor = accentColor;
            notification.Icon = icon;
            notification.IsOpen = true;
        });

        _ = CloseLaterAsync(_closeCts.Token);
    }

    private async Task CloseLaterAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(3500, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                notification.IsOpen = false;
            });
        }
        catch (TaskCanceledException)
        {
        }
    }
}
using System.Windows;
using System.Windows.Controls;

namespace RentalPro.Presentation.Desktop.Common;

public static class PasswordBoxAssistant
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxAssistant),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnBoundPasswordChanged));

    public static readonly DependencyProperty BindPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BindPassword",
            typeof(bool),
            typeof(PasswordBoxAssistant),
            new PropertyMetadata(false, OnBindPasswordChanged));

    private static readonly DependencyProperty UpdatingPasswordProperty =
        DependencyProperty.RegisterAttached(
            "UpdatingPassword",
            typeof(bool),
            typeof(PasswordBoxAssistant),
            new PropertyMetadata(false));

    public static string GetBoundPassword(DependencyObject obj)
    {
        return (string)obj.GetValue(BoundPasswordProperty);
    }

    public static void SetBoundPassword(DependencyObject obj, string value)
    {
        obj.SetValue(BoundPasswordProperty, value);
    }

    public static bool GetBindPassword(DependencyObject obj)
    {
        return (bool)obj.GetValue(BindPasswordProperty);
    }

    public static void SetBindPassword(DependencyObject obj, bool value)
    {
        obj.SetValue(BindPasswordProperty, value);
    }

    private static bool GetUpdatingPassword(DependencyObject obj)
    {
        return (bool)obj.GetValue(UpdatingPasswordProperty);
    }

    private static void SetUpdatingPassword(DependencyObject obj, bool value)
    {
        obj.SetValue(UpdatingPasswordProperty, value);
    }

    private static void OnBindPasswordChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not PasswordBox passwordBox)
            return;

        if ((bool)e.OldValue)
        {
            passwordBox.PasswordChanged -= HandlePasswordChanged;
        }

        if ((bool)e.NewValue)
        {
            passwordBox.PasswordChanged += HandlePasswordChanged;
        }
    }

    private static void OnBoundPasswordChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not PasswordBox passwordBox)
            return;

        passwordBox.PasswordChanged -= HandlePasswordChanged;

        if (!GetUpdatingPassword(passwordBox))
        {
            passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;
        }

        passwordBox.PasswordChanged += HandlePasswordChanged;
    }

    private static void HandlePasswordChanged(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
            return;

        SetUpdatingPassword(passwordBox, true);
        SetBoundPassword(passwordBox, passwordBox.Password);
        SetUpdatingPassword(passwordBox, false);
    }
}
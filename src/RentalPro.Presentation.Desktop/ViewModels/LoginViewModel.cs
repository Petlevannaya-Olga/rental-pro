using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Auth;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class LoginViewModel(
    AuthApiClient authApiClient,
    TokenStorage tokenStorage,
    IServiceProvider serviceProvider)
    : ObservableObject
{
    [ObservableProperty]
    private string _login = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe = true;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Login))
        {
            ErrorMessage = "Введите логин";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введите пароль";
            return;
        }

        try
        {
            IsLoading = true;

            var result = await authApiClient.LoginAsync(Login, Password);

            if (result is null || string.IsNullOrWhiteSpace(result.Token))
            {
                ErrorMessage = "Неверный логин или пароль";
                return;
            }

            tokenStorage.SetToken(result.Token);

            var mainWindow = serviceProvider
                .GetRequiredService<MainWindow>();

            mainWindow.Show();

            foreach (Window window in Application.Current.Windows)
            {
                if (window is LoginWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
        catch
        {
            ErrorMessage = "Не удалось подключиться к серверу";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
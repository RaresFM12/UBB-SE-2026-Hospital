using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Desktop.Auth;

namespace Hospital.Desktop.ViewModels;

public partial class LoginViewModel(AuthClient authClient) : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public event Action? LoginSucceeded;

    private bool CanLogin()
        => !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            await authClient.LoginAsync(Email, Password);
            LoginSucceeded?.Invoke();
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = "Invalid email or password.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not reach the server: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

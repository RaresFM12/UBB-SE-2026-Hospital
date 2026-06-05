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
    public partial string? Email { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string? Password { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

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

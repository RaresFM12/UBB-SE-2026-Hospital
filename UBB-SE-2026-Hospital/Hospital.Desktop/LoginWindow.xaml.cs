using Hospital.Desktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Hospital.Desktop;

public sealed partial class LoginWindow : Window
{
    public LoginViewModel ViewModel { get; }

    public LoginWindow(LoginViewModel viewModel)
    {
        ViewModel = viewModel;
        Title = "Hospital Login";
        InitializeComponent();
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(ViewModel.IsBusy))
            {
                BusyRing.Visibility = ViewModel.IsBusy ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (e.PropertyName is nameof(ViewModel.ErrorMessage))
            {
                ErrorText.Text = ViewModel.ErrorMessage ?? string.Empty;
                ErrorText.Visibility = string.IsNullOrEmpty(ViewModel.ErrorMessage)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        };
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
        => ViewModel.Password = ((PasswordBox)sender).Password;

    private void OnSignInClick(object sender, RoutedEventArgs e)
    {
        ViewModel.Email = EmailBox.Text;
        ViewModel.Password = PasswordInput.Password;

        if (ViewModel.LoginCommand.CanExecute(null))
        {
            ViewModel.LoginCommand.Execute(null);
        }
    }
}

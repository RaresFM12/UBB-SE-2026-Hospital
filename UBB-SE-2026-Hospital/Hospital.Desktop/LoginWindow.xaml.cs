using Hospital.Desktop.ViewModels;
using Microsoft.UI.Xaml;

namespace Hospital.Desktop;

public sealed partial class LoginWindow : Window
{
    public LoginViewModel ViewModel { get; }

    public LoginWindow(LoginViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }
}

using Microsoft.UI.Xaml;

namespace Hospital.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnLogoutClick(object sender, RoutedEventArgs e)
        => ((App)Application.Current).Logout(this);
}

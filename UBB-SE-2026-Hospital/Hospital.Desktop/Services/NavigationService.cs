using Microsoft.UI.Xaml.Controls;

namespace Hospital.Desktop.Services;

public class NavigationService
{
    private Frame? _frame;

    public string CurrentSection { get; private set; } = "Dashboard";

    public void SetFrame(Frame frame)
        => _frame = frame;

    public void Navigate(string sectionName)
    {
        CurrentSection = sectionName;
        // Page navigation is handled in MainWindow.xaml.cs switch
    }

    public void NavigateToPage(Type pageType)
    {
        _frame?.Navigate(pageType);
    }
}

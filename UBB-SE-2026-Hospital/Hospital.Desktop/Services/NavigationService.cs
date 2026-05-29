namespace Hospital.Desktop.Services;

public class NavigationService
{
    public string CurrentSection { get; private set; } = "Dashboard";

    public void Navigate(string sectionName)
        => CurrentSection = sectionName;
}

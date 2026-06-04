using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media;

namespace Hospital.Desktop;

public partial class MainWindow : Window
{
    private readonly Frame contentFrame = new();
    private readonly TextBlock currentPageTitle = new()
    {
        Text = "Dashboard",
        FontSize = 22,
        FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
        Margin = new Thickness(12, 12, 12, 0),
    };

    public MainWindow()
    {
        Title = "Hospital Desktop";
        Content = BuildShell();
        if (Content is UIElement rootElement)
        {
            rootElement.AddHandler(
                UIElement.PointerWheelChangedEvent,
                new PointerEventHandler(OnPointerWheelChanged),
                true);
        }
        contentFrame.Navigated += OnContentFrameNavigated;
        contentFrame.NavigationFailed += OnContentFrameNavigationFailed;
        contentFrame.Navigate(typeof(Views.DashboardPage));
    }

    private Grid BuildShell()
    {
        currentPageTitle.Foreground = (Brush)Application.Current.Resources["AppTextPrimaryBrush"];

        var root = new Grid
        {
            Background = (Brush)Application.Current.Resources["AppBackgroundBrush"],
        };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(260) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var navigation = new ScrollViewer
        {
            Content = BuildNavigation(),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Background = (Brush)Application.Current.Resources["AppSurfaceBrush"],
        };

        var contentHost = new Grid
        {
            Background = (Brush)Application.Current.Resources["AppBackgroundBrush"],
        };
        contentHost.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        contentHost.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        contentFrame.Padding = new Thickness(12);
        Grid.SetRow(contentFrame, 1);
        contentHost.Children.Add(currentPageTitle);
        contentHost.Children.Add(contentFrame);

        Grid.SetColumn(navigation, 0);
        Grid.SetColumn(contentHost, 1);
        root.Children.Add(navigation);
        root.Children.Add(contentHost);

        return root;
    }

    private StackPanel BuildNavigation()
    {
        var panel = new StackPanel { Padding = new Thickness(12), Spacing = 6 };
        AddNavigationButton(panel, "Dashboard", "Dashboard");

        AddSectionHeader(panel, "Admin & Client", "AdminAccounts");
        AddNavigationButton(panel, "Accounts", "AdminAccounts");
        AddNavigationButton(panel, "Appointments", "Appointments");
        AddNavigationButton(panel, "ER Dispatch", "ERDispatch");
        AddNavigationButton(panel, "Fatigue Audit", "FatigueAudit");
        AddNavigationButton(panel, "Admin Schedule", "AdminSchedule");
        AddNavigationButton(panel, "Admin Shift", "AdminShift");

        AddSectionHeader(panel, "Emergency Room", "Triage");
        AddNavigationButton(panel, "Triage", "Triage");
        AddNavigationButton(panel, "Queue", "Queue");
        AddNavigationButton(panel, "Patient Registration", "PatientRegistration");
        AddNavigationButton(panel, "Room Management", "RoomManagement");
        AddNavigationButton(panel, "Room Assignment", "RoomAssignment");
        AddNavigationButton(panel, "Examination", "Examination");
        AddNavigationButton(panel, "Transfer Log", "TransferLog");

        AddSectionHeader(panel, "Patient & Billing", "Patients");
        AddNavigationButton(panel, "Patients", "Patients");
        AddNavigationButton(panel, "Prescriptions", "Prescriptions");
        AddNavigationButton(panel, "Transplants", "Transplants");
        AddNavigationButton(panel, "Blood Donors", "BloodDonors");
        AddNavigationButton(panel, "Statistics", "Statistics");
        AddNavigationButton(panel, "Billing", "Billing");
        AddNavigationButton(panel, "Addict Detection", "AddictDetection");

        panel.Children.Add(new Button
        {
            Content = "Logout",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 12, 0, 0),
        });
        ((Button)panel.Children[^1]).Click += OnLogoutClick;

        return panel;
    }

    private void AddSectionHeader(StackPanel panel, string text, string tag)
    {
        var button = new Button
        {
            Content = text,
            Tag = tag,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 14, 0, 4),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["AppTextSecondaryBrush"],
        };
        button.Click += OnNavigationButtonClick;
        panel.Children.Add(button);
    }

    private void AddNavigationButton(StackPanel panel, string label, string tag)
    {
        var button = new Button
        {
            Content = label,
            Tag = tag,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            MinHeight = 36,
            Foreground = (Brush)Application.Current.Resources["AppTextPrimaryBrush"],
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
            BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
        };
        button.Click += OnNavigationButtonClick;
        panel.Children.Add(button);
    }

    private void OnNavigationButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string tag, Content: string label })
        {
            NavigateToTag(tag, label);
        }
    }

    private void NavigateToTag(string tag, string label)
    {
        Type? pageType = tag switch
        {
            "Dashboard" => typeof(Views.DashboardPage),
            "AdminAccounts" => typeof(Views.Accounts.AdminAccountsManagementView),
            "Appointments" => typeof(Views.Admin.AppointmentsPage),
            "ERDispatch" => typeof(Views.Admin.ERDispatchPage),
            "FatigueAudit" => typeof(Views.Admin.FatigueAuditPage),
            "AdminSchedule" => typeof(Views.Admin.AdminSchedulePage),
            "AdminShift" => typeof(Views.Admin.AdminShiftView),
            "Triage" => typeof(Views.ER.TriagePage),
            "Queue" => typeof(Views.ER.QueuePage),
            "PatientRegistration" => typeof(Views.ER.PatientRegistrationPage),
            "RoomManagement" => typeof(Views.ER.RoomManagementPage),
            "RoomAssignment" => typeof(Views.ER.RoomAssignmentPage),
            "Examination" => typeof(Views.ER.ExaminationPage),
            "TransferLog" => typeof(Views.ER.TransferLogPage),
            "Patients" => typeof(Views.Patient.PatientsPage),
            "Prescriptions" => typeof(Views.Patient.PrescriptionsPage),
            "Transplants" => typeof(Views.Patient.TransplantsPage),
            "BloodDonors" => typeof(Views.Patient.BloodDonorsPage),
            "Statistics" => typeof(Views.Patient.StatisticsPage),
            "Billing" => typeof(Views.Patient.BillingPage),
            "AddictDetection" => typeof(Views.Patient.AddictDetectionPage),
            _ => null,
        };

        if (pageType is null)
        {
            ShowNavigationError(label, "No page is registered for this navigation item.");
            return;
        }

        try
        {
            currentPageTitle.Text = label;
            if (!contentFrame.Navigate(pageType))
            {
                ShowNavigationError(label, $"Frame refused navigation to {pageType.FullName}.");
            }
        }
        catch (Exception ex)
        {
            ShowNavigationError(label, ex.Message);
        }
    }

    private static async Task ShowComingSoonAsync(string tag)
    {
        var dialog = new ContentDialog
        {
            Title = "Coming Soon",
            Content = $"The '{tag}' feature is under development.",
            CloseButtonText = "OK",
            XamlRoot = ((App)Application.Current).CurrentWindow?.Content?.XamlRoot,
        };
        await dialog.ShowAsync();
    }

    private void OnContentFrameNavigated(object sender, NavigationEventArgs e)
    {
    }

    private void OnContentFrameNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        e.Handled = true;
        ShowNavigationError(currentPageTitle.Text, e.Exception.Message);
    }

    private void ShowNavigationError(string label, string message)
    {
        currentPageTitle.Text = label;
        contentFrame.Content = new TextBlock
        {
            Text = $"Could not open {label}.\n\n{message}",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(16),
            Foreground = (Brush)Application.Current.Resources["AppErrorBrush"],
        };
    }

    private void OnLogoutClick(object sender, RoutedEventArgs e)
        => ((App)Application.Current).Logout(this);

    private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source)
        {
            return;
        }

        ScrollViewer? scrollViewer = FindAncestor<ScrollViewer>(source);
        if (scrollViewer is null || scrollViewer.ScrollableHeight <= 0)
        {
            return;
        }

        int delta = e.GetCurrentPoint(scrollViewer).Properties.MouseWheelDelta;
        if (delta == 0)
        {
            return;
        }

        double nextOffset = scrollViewer.VerticalOffset - (delta / 3.0);
        nextOffset = Math.Clamp(nextOffset, 0, scrollViewer.ScrollableHeight);

        if (Math.Abs(nextOffset - scrollViewer.VerticalOffset) < 0.1)
        {
            return;
        }

        scrollViewer.ChangeView(scrollViewer.HorizontalOffset, nextOffset, null, true);
        e.Handled = true;
    }

    private static T? FindAncestor<T>(DependencyObject start) where T : DependencyObject
    {
        DependencyObject? current = start;
        while (current is not null)
        {
            if (current is T match)
            {
                return match;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media;
using Hospital.Shared.Services;
using Hospital.Shared.Models;

namespace Hospital.Desktop;

public partial class MainWindow : Window
{
    private readonly ICurrentUserService currentUserService;
    private readonly Frame contentFrame = new();
    private readonly TextBlock currentPageTitle = new()
    {
        Text = "Dashboard",
        FontSize = 22,
        FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
        Margin = new Thickness(12, 12, 12, 0),
    };

    public MainWindow(ICurrentUserService currentUserService)
    {
        this.currentUserService = currentUserService;
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

        // Section: Admin & Client
        bool showAdminClientHeader = IsFeatureVisibleForRole("AdminAccounts", currentUserService.RoleType)
            || IsFeatureVisibleForRole("Appointments", currentUserService.RoleType)
            || IsFeatureVisibleForRole("ERDispatch", currentUserService.RoleType)
            || IsFeatureVisibleForRole("FatigueAudit", currentUserService.RoleType)
            || IsFeatureVisibleForRole("AdminSchedule", currentUserService.RoleType)
            || IsFeatureVisibleForRole("DoctorSchedule", currentUserService.RoleType)
            || IsFeatureVisibleForRole("PharmacySchedule", currentUserService.RoleType)
            || IsFeatureVisibleForRole("Inventory", currentUserService.RoleType)
            || IsFeatureVisibleForRole("AdminShift", currentUserService.RoleType);
            
        if (showAdminClientHeader)
        {
            AddSectionHeader(panel, "Admin & Client", "AdminAccounts");
            if (IsFeatureVisibleForRole("AdminAccounts", currentUserService.RoleType)) AddNavigationButton(panel, "Accounts", "AdminAccounts");
            if (IsFeatureVisibleForRole("Appointments", currentUserService.RoleType)) AddNavigationButton(panel, "Appointments", "Appointments");
            if (IsFeatureVisibleForRole("DoctorSchedule", currentUserService.RoleType)) AddNavigationButton(panel, "Doctor Schedule", "DoctorSchedule");
            if (IsFeatureVisibleForRole("PharmacySchedule", currentUserService.RoleType)) AddNavigationButton(panel, "Pharmacy Schedule", "PharmacySchedule");
            if (IsFeatureVisibleForRole("Inventory", currentUserService.RoleType)) AddNavigationButton(panel, "Inventory", "Inventory");
            if (IsFeatureVisibleForRole("ERDispatch", currentUserService.RoleType)) AddNavigationButton(panel, "ER Dispatch", "ERDispatch");
            if (IsFeatureVisibleForRole("FatigueAudit", currentUserService.RoleType)) AddNavigationButton(panel, "Fatigue Audit", "FatigueAudit");
            if (IsFeatureVisibleForRole("AdminSchedule", currentUserService.RoleType)) AddNavigationButton(panel, "Admin Schedule", "AdminSchedule");
            if (IsFeatureVisibleForRole("AdminShift", currentUserService.RoleType)) AddNavigationButton(panel, "Admin Shift", "AdminShift");
        }

        // Section: Emergency Room
        bool showERHeader = IsFeatureVisibleForRole("Triage", currentUserService.RoleType)
            || IsFeatureVisibleForRole("Queue", currentUserService.RoleType)
            || IsFeatureVisibleForRole("PatientRegistration", currentUserService.RoleType)
            || IsFeatureVisibleForRole("RoomManagement", currentUserService.RoleType)
            || IsFeatureVisibleForRole("RoomAssignment", currentUserService.RoleType)
            || IsFeatureVisibleForRole("Examination", currentUserService.RoleType)
            || IsFeatureVisibleForRole("TransferLog", currentUserService.RoleType);

        if (showERHeader)
        {
            AddSectionHeader(panel, "Emergency Room", "Triage");
            if (IsFeatureVisibleForRole("Triage", currentUserService.RoleType)) AddNavigationButton(panel, "Triage", "Triage");
            if (IsFeatureVisibleForRole("Queue", currentUserService.RoleType)) AddNavigationButton(panel, "Queue", "Queue");
            if (IsFeatureVisibleForRole("PatientRegistration", currentUserService.RoleType)) AddNavigationButton(panel, "Patient Registration", "PatientRegistration");
            if (IsFeatureVisibleForRole("RoomManagement", currentUserService.RoleType)) AddNavigationButton(panel, "Room Management", "RoomManagement");
            if (IsFeatureVisibleForRole("RoomAssignment", currentUserService.RoleType)) AddNavigationButton(panel, "Room Assignment", "RoomAssignment");
            if (IsFeatureVisibleForRole("Examination", currentUserService.RoleType)) AddNavigationButton(panel, "Examination", "Examination");
            if (IsFeatureVisibleForRole("TransferLog", currentUserService.RoleType)) AddNavigationButton(panel, "Transfer Log", "TransferLog");
        }

        // Section: Patient & Billing
        bool showPatientBillingHeader = IsFeatureVisibleForRole("Patients", currentUserService.RoleType)
            || IsFeatureVisibleForRole("Prescriptions", currentUserService.RoleType)
            || IsFeatureVisibleForRole("Transplants", currentUserService.RoleType)
            || IsFeatureVisibleForRole("BloodDonors", currentUserService.RoleType)
            || IsFeatureVisibleForRole("Statistics", currentUserService.RoleType)
            || IsFeatureVisibleForRole("Billing", currentUserService.RoleType)
            || IsFeatureVisibleForRole("AddictDetection", currentUserService.RoleType);

        if (showPatientBillingHeader)
        {
            AddSectionHeader(panel, "Patient & Billing", "Patients");
            if (IsFeatureVisibleForRole("Patients", currentUserService.RoleType)) AddNavigationButton(panel, "Patients", "Patients");
            if (IsFeatureVisibleForRole("Prescriptions", currentUserService.RoleType)) AddNavigationButton(panel, "Prescriptions", "Prescriptions");
            if (IsFeatureVisibleForRole("Transplants", currentUserService.RoleType)) AddNavigationButton(panel, "Transplants", "Transplants");
            if (IsFeatureVisibleForRole("BloodDonors", currentUserService.RoleType)) AddNavigationButton(panel, "Blood Donors", "BloodDonors");
            if (IsFeatureVisibleForRole("Statistics", currentUserService.RoleType)) AddNavigationButton(panel, "Statistics", "Statistics");
            if (IsFeatureVisibleForRole("Billing", currentUserService.RoleType)) AddNavigationButton(panel, "Billing", "Billing");
            if (IsFeatureVisibleForRole("AddictDetection", currentUserService.RoleType)) AddNavigationButton(panel, "Addict Detection", "AddictDetection");
        }

        // Section: Personal
        if (IsFeatureVisibleForRole("ProfileManagement", currentUserService.RoleType))
        {
            AddSectionHeader(panel, "Personal", "ProfileManagement");
            AddNavigationButton(panel, "Profile", "ProfileManagement");
        }

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
        if (!IsFeatureVisibleForRole(tag, this.currentUserService.RoleType))
        {
            ShowNavigationError(label, "Access Denied: You do not have permission to view this section.");
            return;
        }

        Type? pageType = tag switch
        {
            "Dashboard" => typeof(Views.DashboardPage),
            "ProfileManagement" => typeof(Views.Accounts.ProfileManagementView),
            "AdminAccounts" => typeof(Views.Accounts.AdminAccountsManagementView),
            "Appointments" => typeof(Views.Admin.AppointmentsPage),
            "DoctorSchedule" => typeof(Views.Doctor.DoctorSchedulePage),
            "PharmacySchedule" => typeof(Views.Pharmacy.PharmacySchedulePage),
            "Inventory" => typeof(Views.PharmacyManagement.EditPage),
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

    private static bool IsFeatureVisibleForRole(string navigationTag, UserRole userRole)
    {
        if (userRole == UserRole.Admin)
        {
            return navigationTag is "Dashboard"
                || navigationTag is "ProfileManagement"
                || navigationTag is "Appointments"
                || navigationTag is "DoctorSchedule"
                || navigationTag is "PharmacySchedule"
                || navigationTag is "Inventory"
                || navigationTag is "AdminAccounts"
                || navigationTag is "ERDispatch"
                || navigationTag is "FatigueAudit"
                || navigationTag is "AdminSchedule"
                || navigationTag is "AdminShift"
                || navigationTag is "Statistics";
        }

        if (userRole == UserRole.Doctor)
        {
            return navigationTag is "Dashboard" 
                || navigationTag is "Appointments" 
                || navigationTag is "Triage" 
                || navigationTag is "Queue" 
                || navigationTag is "PatientRegistration"
                || navigationTag is "RoomManagement" 
                || navigationTag is "RoomAssignment" 
                || navigationTag is "Examination" 
                || navigationTag is "TransferLog" 
                || navigationTag is "Patients" 
                || navigationTag is "Transplants" 
                || navigationTag is "BloodDonors";
        }

        if (userRole == UserRole.Pharmacist)
        {
            return navigationTag is "Dashboard" 
                || navigationTag is "Prescriptions" 
                || navigationTag is "AddictDetection";
        }

        if (userRole == UserRole.Client)
        {
            return navigationTag is "Dashboard" 
                || navigationTag is "Appointments" 
                || navigationTag is "Billing" 
                || navigationTag is "Prescriptions";
        }

        return false;
    }
}

using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media;
using Hospital.Shared.Services;
using Hospital.Data.Models;
using Hospital.Shared.Enums;

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

        // Top-level Dashboard link, mirroring the web navbar.
        AddNavigationButton(panel, "Dashboard", "Dashboard");

        // The categories below mirror the dropdowns in the web navbar
        // (Hospital.Web/Views/Shared/_Layout.cshtml). Only items whose page
        // exists in the desktop app and is visible to the current role appear.

        AddCategory(panel, "Patient Care",
            ("", new[]
            {
                ("Patients", "Patients"),
                ("Registration", "PatientRegistration"),
                ("Consultations", "Consultations"),
                ("Medical Evaluations", "MedicalEvaluations"),
                ("Queue", "Queue"),
                ("Triage", "Triage"),
                ("Examination", "Examination"),
                ("Period Tracker", "PeriodTracker"),
            }));

        AddCategory(panel, "Facilities & Depts",
            ("Rooms & Movement", new[]
            {
                ("Room Management", "RoomManagement"),
                ("Room Assignment", "RoomAssignment"),
                ("Transfer", "TransferLog"),
            }),
            ("Specialized", new[]
            {
                ("Blood Compatibility", "BloodDonors"),
                ("Organ Donor", "OrganDonor"),
                ("Transplant", "Transplants"),
                ("Ghost", "Ghost"),
            }));

        AddCategory(panel, "Pharmacy",
            ("", new[]
            {
                ("Prescriptions", "Prescriptions"),
                ("Product Catalogue", "Inventory"),
                ("Orders", "Orders"),
                ("Addict Detection", "AddictDetection"),
            }),
            ("Client Tools", new[]
            {
                ("My Basket", "Basket"),
                ("Billing", "Billing"),
            }));

        AddCategory(panel, "Staff Portal",
            ("Doctor Actions", new[]
            {
                ("My Appointments", "Appointments"),
                ("My Schedule", "DoctorSchedule"),
                ("Hangouts", "Hangouts"),
                ("Shift Swap Requests", "ShiftSwapRequests"),
                ("Incoming Swaps", "IncomingSwaps"),
            }),
            ("Pharmacy Actions", new[]
            {
                ("Schedule", "PharmacySchedule"),
            }),
            ("Management & Pay", new[]
            {
                ("Shift Management", "AdminSchedule"),
                ("Create Shift", "AdminShift"),
            }));

        AddCategory(panel, "Admin",
            ("", new[]
            {
                ("Modules", "Modules"),
                ("Medical Staff", "MedicalStaff"),
                ("User Accounts", "AdminAccounts"),
                ("System Notifications", "Notifications"),
            }),
            ("Data & Logistics", new[]
            {
                ("Statistics", "Statistics"),
                ("Inventory Items", "InventoryItems"),
                ("Substances", "Substances"),
                ("ER Dispatch", "ERDispatch"),
                ("Fatigue Audit", "FatigueAudit"),
            }));

        // Personal: profile link (the web navbar shows this near the username).
        if (IsFeatureVisibleForRole("ProfileManagement", currentUserService.RoleType))
        {
            AddNavigationButton(panel, "Profile", "ProfileManagement");
        }

        var logout = new Button
        {
            Content = "Logout",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 12, 0, 0),
        };
        logout.Click += OnLogoutClick;
        panel.Children.Add(logout);

        return panel;
    }

    // Builds one collapsible category (the desktop equivalent of a web navbar
    // dropdown). Each group is an optional sub-header plus its items; an item is
    // only shown if its page is visible to the current role. The category is
    // omitted entirely when none of its items are visible.
    private void AddCategory(
        StackPanel panel,
        string header,
        params (string SubHeader, (string Label, string Tag)[] Items)[] groups)
    {
        var content = new StackPanel { Spacing = 2, Margin = new Thickness(4, 0, 0, 0) };

        foreach (var (subHeader, items) in groups)
        {
            var visible = items
                .Where(item => IsFeatureVisibleForRole(item.Tag, currentUserService.RoleType))
                .ToArray();

            if (visible.Length == 0)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(subHeader))
            {
                AddSubGroupHeader(content, subHeader);
            }

            foreach (var (label, tag) in visible)
            {
                AddNavigationButton(content, label, tag);
            }
        }

        if (content.Children.Count == 0)
        {
            return;
        }

        panel.Children.Add(new Expander
        {
            Header = new TextBlock
            {
                Text = header,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = (Brush)Application.Current.Resources["AppTextPrimaryBrush"],
            },
            Content = content,
            IsExpanded = true,
            Margin = new Thickness(0, 6, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        });
    }

    private void AddSubGroupHeader(StackPanel panel, string text)
    {
        panel.Children.Add(new TextBlock
        {
            Text = text,
            FontSize = 12,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["AppTextSecondaryBrush"],
            Margin = new Thickness(4, 8, 0, 2),
        });
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
            "Consultations" => typeof(Views.Patient.ConsultationsPage),
            "MedicalEvaluations" => typeof(Views.Patient.MedicalEvaluationsPage),
            "OrganDonor" => typeof(Views.Patient.OrganDonorPage),
            "Ghost" => typeof(Views.Admin.GhostPage),
            "Orders" => typeof(Views.Pharmacy.OrdersPage),
            "Basket" => typeof(Views.Pharmacy.BasketPage),
            "Hangouts" => typeof(Views.Doctor.HangoutsPage),
            "ShiftSwapRequests" => typeof(Views.Doctor.ShiftSwapRequestsPage),
            "IncomingSwaps" => typeof(Views.Doctor.IncomingSwapsPage),
            "MedicalStaff" => typeof(Views.Admin.MedicalStaffPage),
            "Modules" => typeof(Views.Admin.ModulesPage),
            "Notifications" => typeof(Views.Admin.NotificationsPage),
            "Substances" => typeof(Views.Admin.SubstancesPage),
            "InventoryItems" => typeof(Views.Admin.InventoryItemsPage),
            "PeriodTracker" => typeof(Views.Patient.PeriodTrackerPage),
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
        // Features the web navbar (_Layout.cshtml) exposes to every authenticated
        // user: the Patient Care, Facilities & Departments, and Pharmacy groups.
        bool isSharedFeature = navigationTag
            is "Dashboard"
            or "ProfileManagement"
            // Patient Care
            or "Patients"
            or "PatientRegistration"
            or "Consultations"
            or "MedicalEvaluations"
            or "Queue"
            or "Triage"
            or "Examination"
            or "PeriodTracker"
            // Facilities & Departments
            or "RoomManagement"
            or "RoomAssignment"
            or "TransferLog"
            or "BloodDonors"
            or "OrganDonor"
            or "Transplants"
            or "Ghost"
            // Pharmacy
            or "Prescriptions"
            or "Inventory"
            or "Orders"
            or "AddictDetection";

        return userRole switch
        {
            // Admin reaches every desktop page, mirroring the web Admin menu.
            UserRole.Admin => true,
            // Doctor: shared features + the web Staff Portal "Doctor Actions".
            UserRole.Doctor => isSharedFeature
                || navigationTag is "Appointments" or "DoctorSchedule"
                or "Hangouts" or "ShiftSwapRequests" or "IncomingSwaps",
            // Pharmacist: shared features + the web Staff Portal "Pharmacy Actions".
            UserRole.Pharmacist => isSharedFeature || navigationTag is "PharmacySchedule",
            // Client: shared features + the web "Client Tools".
            UserRole.Client => isSharedFeature || navigationTag is "Billing" or "Basket",
            _ => false,
        };
    }
}

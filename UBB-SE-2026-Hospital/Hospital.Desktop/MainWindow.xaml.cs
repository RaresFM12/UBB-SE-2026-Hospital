using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Hospital.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ContentFrame.Navigated += OnContentFrameNavigated;
    }

    private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            _ = NavigateToTagAsync(tag);
        }
    }

    private async Task NavigateToTagAsync(string tag)
    {
        var services = App.Services;
        switch (tag)
        {
            case "Dashboard":
                ContentFrame.Navigate(typeof(Views.DashboardPage));
                break;
            case "AdminAccounts":
                ContentFrame.Navigate(typeof(Views.Accounts.AdminAccountsManagementView));
                break;
            case "Appointments":
                ContentFrame.Navigate(typeof(Views.Admin.AppointmentsPage));
                break;
            case "ERDispatch":
                ContentFrame.Navigate(typeof(Views.Admin.ERDispatchPage));
                break;
            case "FatigueAudit":
                ContentFrame.Navigate(typeof(Views.Admin.FatigueAuditPage));
                break;
            case "AdminSchedule":
                ContentFrame.Navigate(typeof(Views.Admin.AdminSchedulePage));
                break;
            case "AdminShift":
                ContentFrame.Navigate(typeof(Views.Admin.AdminShiftView));
                break;
            case "Triage":
                ContentFrame.Navigate(typeof(Views.ER.TriagePage));
                break;
            case "Queue":
                ContentFrame.Navigate(typeof(Views.ER.QueuePage));
                break;
            case "PatientRegistration":
                ContentFrame.Navigate(typeof(Views.ER.PatientRegistrationPage));
                break;
            case "RoomManagement":
                ContentFrame.Navigate(typeof(Views.ER.RoomManagementPage));
                break;
            case "RoomAssignment":
                ContentFrame.Navigate(typeof(Views.ER.RoomAssignmentPage));
                break;
            case "Examination":
                ContentFrame.Navigate(typeof(Views.ER.ExaminationPage));
                break;
            case "TransferLog":
                ContentFrame.Navigate(typeof(Views.ER.TransferLogPage));
                break;
            case "Patients":
                ContentFrame.Navigate(typeof(Views.Patient.PatientsPage));
                break;
            case "Prescriptions":
                ContentFrame.Navigate(typeof(Views.Patient.PrescriptionsPage));
                break;
            case "Transplants":
                ContentFrame.Navigate(typeof(Views.Patient.TransplantsPage));
                break;
            case "BloodDonors":
                ContentFrame.Navigate(typeof(Views.Patient.BloodDonorsPage));
                break;
            case "Statistics":
                ContentFrame.Navigate(typeof(Views.Patient.StatisticsPage));
                break;
            case "Billing":
                ContentFrame.Navigate(typeof(Views.Patient.BillingPage));
                break;
            case "AddictDetection":
                ContentFrame.Navigate(typeof(Views.Patient.AddictDetectionPage));
                break;
            default:
                await ShowComingSoonAsync(tag);
                break;
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
        // Could update selected NavigationView item here if needed
    }

    private void OnLogoutClick(object sender, RoutedEventArgs e)
        => ((App)Application.Current).Logout(this);
}

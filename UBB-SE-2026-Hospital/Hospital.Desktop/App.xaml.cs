using Hospital.Desktop.Auth;
using Hospital.Desktop.Services;
using Hospital.Desktop.ViewModels.Accounts; // removed to avoid ambiguity
using Hospital.Desktop.ViewModels.Admin;
using Hospital.Desktop.ViewModels.Base;
using Hospital.Desktop.ViewModels.Doctor;
using Hospital.Desktop.ViewModels.ER;
using Hospital.Desktop.ViewModels.Patient;
using Hospital.Desktop.ViewModels.Pharmacy;
using Hospital.Desktop.ViewModels.PharmacyManagement;
using Hospital.Desktop.Views.Shell;
using Hospital.Shared.Proxies;
using Hospital.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Net.Http;


//using Hospital.Desktop.ViewModels;

namespace Hospital.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public Window? CurrentWindow { get; private set; }

    public App()
    {
        InitializeComponent();
        UnhandledException += (_, e) => LogException(e.Exception);

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        string apiBaseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5106";

        // JWT auth handler + named HttpClient
        //services.AddTransient<JwtAuthHandler>();
        services.AddHttpClient("api", c =>
        {
            c.BaseAddress = new Uri(apiBaseUrl);
            c.Timeout = TimeSpan.FromSeconds(10);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
        services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("api"));
        var apiBaseUri = new Uri(apiBaseUrl);
        // Register shared business logic services

        
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<AuthClient>();
        services.AddSingleton<NavigationService>();

        // Async proxies (House-MD + MysteryInc)
        services.AddSingleton<IPatientService, PatientApiClient>();
        services.AddSingleton<IERRoomService, ERRoomApiClient>();
        services.AddSingleton<IERVisitService, ERVisitApiClient>();
        services.AddSingleton<ITriageService, TriageApiClient>();
        services.AddSingleton<IExaminationService, ExaminationApiClient>();
        services.AddSingleton<ITransferLogService, TransferLogApiClient>();
        services.AddSingleton<IDoctorAppointmentService, DoctorAppointmentApiClient>();
        services.AddSingleton<IERDispatchService, ERDispatchApiClient>();
        services.AddSingleton<IAllergyService, AllergyApiClient>();
        services.AddSingleton<IStatisticsService, StatisticsApiClient>();
        services.AddSingleton<ITransplantService, TransplantApiClient>();
        services.AddSingleton<IBloodCompatibilityService, BloodCompatibilityApiClient>();
        services.AddSingleton<IBillingService, BillingApiClient>();
        services.AddSingleton<IAddictDetectionService, AddictDetectionApiClient>();
        services.AddSingleton<PrescriptionApiClient>();
        services.AddSingleton<IPrescriptionService>(serviceProvider => serviceProvider.GetRequiredService<PrescriptionApiClient>());
        services.AddSingleton<IPharmacyScheduleService, PharmacyScheduleApiClient>();
        services.AddSingleton<IPharmacyVacationService, PharmacyVacationApiClient>();

        // Sync-blocking proxies (923-2 admin/client)
        services.AddSingleton<IAdminService, AdminApiClient>();
        services.AddSingleton<IOrderService, OrdersApiClient>();
        services.AddSingleton<IUserAccountService, UserAccountApiClient>();
        services.AddHttpClient<IShiftManagementService, ShiftManagementApiClient>("api");
        services.AddSingleton<IFatigueAuditService, FatigueAuditApiClient>();
        services.AddHttpClient<IPharmacyScheduleService, PharmacyScheduleApiClient>("api");

        // Newly ported web features (desktop parity)
        services.AddSingleton<IGhostApiClient, GhostApiClient>();
        services.AddSingleton<IMedicalEvaluationService, MedicalEvaluationApiClient>();
        services.AddHttpClient<IHangoutService, HangoutApiClient>("api");
        services.AddHttpClient<IPharmacyScheduleService, PharmacyScheduleApiClient>("api");
        services.AddSingleton<IShiftSwapService, ShiftSwapApiClient>();
        services.AddSingleton<INotificationService, NotificationApiClient>();

        // ViewModels
        // Removed incorrect ViewModel registration; EditPageViewModel is registered later
        services.AddTransient<Hospital.Desktop.ViewModels.LoginViewModel>();
        services.AddTransient<AdminAccountsManagementViewModel>();
        services.AddTransient<AdminAppointmentsViewModel>();
        services.AddTransient<AdminShiftViewModel>();
        services.AddTransient<ERDispatchViewModel>();
        services.AddTransient<FatigueShiftAuditViewModel>();
        services.AddTransient<TriageViewModel>();
        services.AddTransient<QueueViewModel>();
        services.AddTransient<PatientRegistrationViewModel>();
        services.AddTransient<RoomManagementViewModel>();
        services.AddTransient<RoomAssignmentViewModel>();
        services.AddTransient<ExaminationViewModel>();
        services.AddTransient<TransferLogViewModel>();

        // Patient & Billing VMs
        services.AddTransient<PatientViewModel>();
        services.AddTransient<BloodDonorsViewModel>();
        services.AddTransient<PrescriptionViewModel>();
        services.AddTransient<TransplantViewModel>();
        services.AddTransient<StatisticsViewModel>();
        services.AddTransient<BillingViewModel>();
        services.AddTransient<AddictDetectionViewModel>();

        // Windows & Pages
        services.AddTransient<LoginWindow>();
            // Ensure LoginViewModel is registered (added via using above)
            // services.AddTransient<LoginViewModel>(); // duplicate removed
        services.AddTransient<MainWindow>();
        services.AddTransient<Views.Patient.PatientProfileWindow>();

        // Migrated ViewModels & Services
        services.AddTransient<Hospital.Desktop.ViewModels.Doctor.DoctorScheduleViewModel>();
        services.AddTransient<Hospital.Desktop.ViewModels.Doctor.AppointmentItemViewModel>();
        services.AddTransient<Hospital.Desktop.ViewModels.Doctor.DoctorShiftItemViewModel>();
        services.AddTransient<Hospital.Desktop.ViewModels.Pharmacy.PharmacyScheduleViewModel>();
        services.AddTransient<Hospital.Desktop.ViewModels.Pharmacy.PharmacyShiftItemViewModel>();
        services.AddTransient<Hospital.Desktop.ViewModels.PharmacyManagement.EditPageViewModel>();

        //the details of the appointments in the doctors schedule did not show up without this line commented out
        //services.AddTransient<Hospital.Desktop.Views.Shell.DialogPresenter>();
        services.AddSingleton<DialogPresenter>();

        var provider = services.BuildServiceProvider();

        Services = provider;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        ShowLogin();
    }

    private void ShowLogin()
    {
        var loginWindow = Services.GetRequiredService<LoginWindow>();
        loginWindow.ViewModel.LoginSucceeded += () =>
        {
            loginWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    var shell = Services.GetRequiredService<MainWindow>();
                    CurrentWindow = shell;
                    shell.Activate();
                    loginWindow.Close();
                }
                catch (Exception ex)
                {
                    LogException(ex);
                    var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
                    {
                        Title = "Launch Error",
                        Content = ex.ToString(),
                        CloseButtonText = "OK",
                        XamlRoot = loginWindow.Content?.XamlRoot,
                    };
                    await dialog.ShowAsync();
                }
            });
        };
        loginWindow.Activate();
    }

    public void Logout(Window current)
    {
        Services.GetRequiredService<AuthClient>().Logout();
        CurrentWindow = null;
        ShowLogin();
        current.Close();
    }

    private static void LogException(Exception ex)
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Hospital.Desktop.crash.log");

        File.AppendAllText(logPath, $"{DateTimeOffset.Now:O}{Environment.NewLine}{ex}{Environment.NewLine}");
    }
}



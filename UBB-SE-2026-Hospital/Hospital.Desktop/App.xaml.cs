using System;
using System.IO;
using System.Net.Http;
using Hospital.Desktop.Auth;
using Hospital.Desktop.Proxy;
using Hospital.Desktop.Services;
using Hospital.Desktop.ViewModels.Accounts;
using Hospital.Desktop.ViewModels.Admin;
using Hospital.Desktop.ViewModels.ER;
using Hospital.Desktop.ViewModels.Patient;
using Hospital.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

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
        services.AddTransient<JwtAuthHandler>();
        services.AddHttpClient("api", c =>
        {
            c.BaseAddress = new Uri(apiBaseUrl);
            c.Timeout = TimeSpan.FromSeconds(10);
        })
                .AddHttpMessageHandler<JwtAuthHandler>();
        services.AddSingleton(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));

        services.AddSingleton<AuthClient>();
        services.AddSingleton<NavigationService>();

        // Async proxies (House-MD + MysteryInc)
        services.AddSingleton<IPatientService, HttpPatientProxy>();
        services.AddSingleton<IERRoomService, HttpERRoomProxy>();
        services.AddSingleton<IERVisitService, HttpERVisitProxy>();
        services.AddSingleton<ITriageService, HttpTriageProxy>();
        services.AddSingleton<IExaminationService, HttpExaminationProxy>();
        services.AddSingleton<ITransferLogService, HttpTransferLogProxy>();
        services.AddSingleton<IDoctorAppointmentService, HttpDoctorAppointmentProxy>();
        services.AddSingleton<IERDispatchService, HttpERDispatchProxy>();
        services.AddSingleton<IAllergyService, HttpAllergyProxy>();
        services.AddSingleton<IStatisticsService, HttpStatisticsProxy>();
        services.AddSingleton<ITransplantService, HttpTransplantProxy>();
        services.AddSingleton<IBloodCompatibilityService, HttpBloodCompatibilityProxy>();
        services.AddSingleton<IBillingService, HttpBillingProxy>();
        services.AddSingleton<IAddictDetectionService, HttpAddictDetectionProxy>();
        services.AddSingleton<HttpPrescriptionProxy>();
        services.AddSingleton<IPrescriptionService>(sp => sp.GetRequiredService<HttpPrescriptionProxy>());

        // Sync-blocking proxies (923-2 admin/client)
        services.AddSingleton<IAdminService, HttpAdminProxy>();
        services.AddSingleton<IOrderService, HttpOrdersProxy>();
        services.AddSingleton<IUserAccountService, HttpUserAccountProxy>();
        services.AddSingleton<IShiftManagementService, HttpShiftManagementProxy>();
        services.AddSingleton<IFatigueAuditService, HttpFatigueAuditProxy>();

        // ViewModels
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
        services.AddTransient<MainWindow>();
        services.AddTransient<Views.Patient.PatientProfileWindow>();

        Services = services.BuildServiceProvider();
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
            loginWindow.DispatcherQueue.TryEnqueue(() =>
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
                    throw;
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

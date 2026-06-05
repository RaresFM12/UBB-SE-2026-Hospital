using System;
using System.Net.Http;
using Hospital.Desktop.Auth;
using Hospital.Desktop.Proxy;
using Hospital.Desktop.ViewModels;
using Hospital.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Hospital.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        InitializeComponent();

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        string apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:7001";
        services.AddSingleton(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

        services.AddSingleton<AuthClient>();
        services.AddTransient<IPatientService, HttpPatientProxy>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();

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
            var shell = Services.GetRequiredService<MainWindow>();
            shell.Activate();
            loginWindow.Close();
        };
        loginWindow.Activate();
    }

    public void Logout(Window current)
    {
        Services.GetRequiredService<AuthClient>().Logout();
        ShowLogin();
        current.Close();
    }
}

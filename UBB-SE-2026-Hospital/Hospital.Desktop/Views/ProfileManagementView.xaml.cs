namespace Hospital.Desktop.Views.Accounts
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Hospital.Shared.Services;
    using Hospital.Desktop.ViewModels.Accounts;

    public sealed partial class ProfileManagementView : Page
    {
        private readonly IUserAccountService accountService;

        public ProfileManagementViewModel ProfileManagementViewModel { get; }

        public ProfileManagementView()
        {
            this.InitializeComponent();

            this.accountService = ServiceWrapper.UserAccountService;
            var currentUserService = App.Services.GetRequiredService<ICurrentUserService>();
            this.ProfileManagementViewModel = new ProfileManagementViewModel(this.accountService, currentUserService);

            this.DataContext = this.ProfileManagementViewModel;
            this.Loaded += this.OnProfileLoaded;
        }

        private void OnProfileLoaded(object sender, RoutedEventArgs e)
        {
            this.ProfileManagementViewModel.LoadUserData();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ProfileManagementViewModel.ErrorMessage = null;
                this.ProfileManagementViewModel.SaveChanges();
            }
            catch (Exception exception)
            {
                this.ProfileManagementViewModel.ErrorMessage = exception.Message;
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            this.ProfileManagementViewModel.CancelChanges();
        }

        private async void OnChangePasswordClick(object sender, RoutedEventArgs e)
        {
            var dialog = new ChangePasswordView(this.accountService);
            dialog.XamlRoot = this.XamlRoot;

            await dialog.ShowAsync();
        }
    }
}

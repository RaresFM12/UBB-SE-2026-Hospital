namespace Hospital.Desktop.ViewModels.Accounts
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Hospital.Data.Models;
    using Hospital.Shared.Services;

    public class ProfileManagementViewModel : INotifyPropertyChanged
    {
        private readonly IUserAccountService userAccountService;
        private readonly ICurrentUserService currentUserService;

        private string username;
        private string phoneNumber;
        private string errorMessage;

        public ProfileManagementViewModel(IUserAccountService userAccountService, ICurrentUserService currentUserService)
        {
            this.userAccountService = userAccountService;
            this.currentUserService = currentUserService;
        }

        public string Email => this.userAccountService.CurrentUser?.Email ?? string.Empty;

        public string Username
        {
            get => this.username;
            set
            {
                this.username = value;
                this.OnPropertyChanged();
            }
        }

        public string PhoneNumber
        {
            get => this.phoneNumber;
            set
            {
                this.phoneNumber = value;
                this.OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => this.errorMessage;
            set
            {
                this.errorMessage = value;
                this.OnPropertyChanged();
            }
        }

        public void LoadUserData()
        {
            this.ErrorMessage = null;
            int userId = this.userAccountService.CurrentUser?.Id ?? this.currentUserService.UserId;
            if (userId <= 0)
            {
                this.ErrorMessage = "No signed-in user was found.";
                return;
            }

            try
            {
                User? currentUser = this.userAccountService.LoadCurrentUser(userId);
                if (currentUser == null)
                {
                    this.ErrorMessage = "Could not load your profile.";
                    return;
                }

                this.OnPropertyChanged(nameof(this.Email));
                this.Username = currentUser.Username;
                this.PhoneNumber = currentUser.PhoneNumber;
            }
            catch (System.Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
        }

        public void SaveChanges()
        {
            try
            {
                this.ErrorMessage = null;
                this.userAccountService.UpdateProfile(this.Username, this.PhoneNumber);
            }
            catch (System.Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
        }

        public void CancelChanges()
        {
            this.LoadUserData();
            this.ErrorMessage = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

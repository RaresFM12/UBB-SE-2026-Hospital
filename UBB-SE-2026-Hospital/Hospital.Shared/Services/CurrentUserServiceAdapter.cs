namespace Hospital.Shared.Services
{
    using Hospital.Shared.Models;

    public class CurrentUserServiceAdapter : RaresICurrentUserService
    {
        public User RaresCurrentUser => ServiceWrapper.UserAccountService.CurrentUser;
    }
}

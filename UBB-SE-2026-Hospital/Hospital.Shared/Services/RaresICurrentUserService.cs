namespace Hospital.Shared.Services
{
    using Hospital.Shared.Models;

    public interface RaresICurrentUserService
    {
        User RaresCurrentUser { get; }
    }
}

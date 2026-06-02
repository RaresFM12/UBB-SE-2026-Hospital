namespace Hospital.Shared.Services
{
    using Microsoft.Extensions.DependencyInjection;
    using Hospital.Shared.Repositories;
    using Hospital.Shared;

    public static class ServiceWrapper
    {
        public static UserAccountService UserAccountService { get; private set; } = null!;

        public static void Initialize()
        {
            // Resolve the users repository from the shared DI container that
            // the host (desktop App or web Program) built before invoking us.
            // The other two collaborators have no I/O and are cheap to
            // instantiate directly.
            IUsersRepository userRepository = SharedServiceProvider.Services.GetRequiredService<IUsersRepository>();
            ISecurityService securityService = new SecurityService();
            IUserValidationService validationService = new UserValidationService();

            UserAccountService = new UserAccountService(userRepository, securityService, validationService);
        }
    }
}

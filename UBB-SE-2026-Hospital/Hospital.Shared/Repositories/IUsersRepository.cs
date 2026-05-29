using Hospital.Shared.Models;

namespace Hospital.Shared.Repositories;

public interface IUsersRepository
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

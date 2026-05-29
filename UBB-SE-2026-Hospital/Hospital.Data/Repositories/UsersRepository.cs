using Hospital.Shared.Models;
using Hospital.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class UsersRepository(HospitalDbContext context) : IUsersRepository
{
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Users.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
}

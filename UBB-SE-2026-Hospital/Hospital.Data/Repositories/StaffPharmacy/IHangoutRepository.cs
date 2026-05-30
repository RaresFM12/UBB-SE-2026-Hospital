using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IHangoutRepository
{
    Task<Hangout?> GetByIdAsync(int hangoutId);
    Task<List<Hangout>> GetAllAsync();
    Task<List<Hangout>> GetByOrganizerIdAsync(int staffId);
    Task<Hangout> CreateAsync(Hangout hangout);
    Task<Hangout> UpdateAsync(Hangout hangout);
    Task DeleteAsync(int hangoutId);
}

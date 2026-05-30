using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IHangoutParticipantRepository
{
    Task<HangoutParticipant?> GetByIdAsync(int hangoutId, int staffId);
    Task<List<HangoutParticipant>> GetByHangoutIdAsync(int hangoutId);
    Task<List<HangoutParticipant>> GetByStaffIdAsync(int staffId);
    Task<HangoutParticipant> CreateAsync(HangoutParticipant participant);
    Task DeleteAsync(int hangoutId, int staffId);
}

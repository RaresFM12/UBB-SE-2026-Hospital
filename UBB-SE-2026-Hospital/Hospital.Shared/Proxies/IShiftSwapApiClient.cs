using Hospital.Data.Models;

namespace Hospital.Shared.Proxies;

public interface IShiftSwapApiClient
{
    Task<IReadOnlyList<ShiftSwapRequest>> GetAllShiftSwapRequestsAsync(CancellationToken cancellationToken = default);
    List<ShiftSwapRequest> GetAllShiftSwapRequests();
    Task<ShiftSwapRequest?> GetShiftSwapByIdAsync(int swapId, CancellationToken cancellationToken = default);
    Task<int> CreateShiftSwapRequestAsync(int shiftId, int requesterId, int colleagueId, DateTime requestedAt, ShiftSwapRequestStatus status, CancellationToken cancellationToken = default);
    void RequestShiftSwap(int requesterId, int shiftId, int colleagueId, out string message);
    Task UpdateShiftSwapStatusAsync(int swapId, string status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Shift>> GetFutureShiftsForStaffAsync(int staffId, CancellationToken cancellationToken = default);
    List<Shift> GetFutureShiftsForStaff(int staffId);
    Task<IReadOnlyList<Staff>> GetEligibleSwapColleaguesAsync(int requesterId, int shiftId, CancellationToken cancellationToken = default);
    List<IStaff> GetEligibleSwapColleaguesForShift(int requesterId, int shiftId, out string error);
    Task<bool> AcceptSwapRequestAsync(int swapId, int colleagueId, CancellationToken cancellationToken = default);
    void AcceptSwapRequest(int swapId, int colleagueId, out string message);
    Task<bool> RejectSwapRequestAsync(int swapId, int colleagueId, CancellationToken cancellationToken = default);
    void RejectSwapRequest(int swapId, int colleagueId, out string message);
    List<Doctor> GetAllDoctors();
    List<ShiftSwapRequest> GetIncomingSwapRequests(int staffId);
}

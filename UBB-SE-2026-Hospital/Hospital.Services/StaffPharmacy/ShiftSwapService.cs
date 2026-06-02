using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class ShiftSwapService(
    IStaffRepository staffRepository,
    IShiftRepository shiftRepository,
    IShiftSwapRepository shiftSwapRepository,
    INotificationRepository notificationRepository) : IShiftSwapService
{
    public async Task<IReadOnlyList<ShiftSwapRequest>> GetAllShiftSwapRequestsAsync(CancellationToken cancellationToken = default)
        => await shiftSwapRepository.GetAllAsync();

    public List<ShiftSwapRequest> GetAllShiftSwapRequests() =>
        GetAllShiftSwapRequestsAsync().GetAwaiter().GetResult().ToList();

    public async Task<ShiftSwapRequest?> GetShiftSwapByIdAsync(int swapId, CancellationToken cancellationToken = default)
        => await shiftSwapRepository.GetByIdAsync(swapId);

    public async Task<int> CreateShiftSwapRequestAsync(int shiftId, int requesterId, int colleagueId, DateTime requestedAt, ShiftSwapRequestStatus status, CancellationToken cancellationToken = default)
    {
        var shift = await shiftRepository.GetByIdAsync(shiftId)
            ?? throw new ArgumentException("Shift not found.");
        var requester = await staffRepository.GetByIdAsync(requesterId)
            ?? throw new ArgumentException("Requester not found.");
        var colleague = await staffRepository.GetByIdAsync(colleagueId)
            ?? throw new ArgumentException("Colleague not found.");

        var request = await shiftSwapRepository.CreateAsync(new ShiftSwapRequest
        {
            Shift = shift,
            Requester = requester,
            Colleague = colleague,
            RequestedAt = requestedAt,
            Status = status,
        });

        await notificationRepository.CreateAsync(new Notification
        {
            Recipient = colleague,
            Title = "New Shift Swap Request",
            Message = $"You received a shift swap request from {requester.FullName} for shift #{shiftId}.",
            CreatedAt = DateTime.UtcNow,
        });

        return request.SwapId;
    }

    public void RequestShiftSwap(int requesterId, int shiftId, int colleagueId, out string message)
    {
        CreateShiftSwapRequestAsync(shiftId, requesterId, colleagueId, DateTime.Now, ShiftSwapRequestStatus.PENDING)
            .GetAwaiter().GetResult();
        message = "Shift swap request created.";
    }

    public async Task UpdateShiftSwapStatusAsync(int swapId, string status, CancellationToken cancellationToken = default)
    {
        var request = await shiftSwapRepository.GetByIdAsync(swapId)
            ?? throw new ArgumentException("Swap request not found.");
        request.Status = Enum.TryParse<ShiftSwapRequestStatus>(status, true, out var parsed)
            ? parsed
            : throw new ArgumentException("Invalid swap status.");
        await shiftSwapRepository.UpdateAsync(request);
    }

    public async Task<IReadOnlyList<Shift>> GetFutureShiftsForStaffAsync(int staffId, CancellationToken cancellationToken = default)
        => (await shiftRepository.GetByStaffIdAsync(staffId))
            .Where(shift => shift.StartTime > DateTime.Now)
            .OrderBy(shift => shift.StartTime)
            .ToList();

    public List<Shift> GetFutureShiftsForStaff(int staffId) =>
        GetFutureShiftsForStaffAsync(staffId).GetAwaiter().GetResult().ToList();

    public async Task<IReadOnlyList<Staff>> GetEligibleSwapColleaguesAsync(int requesterId, int shiftId, CancellationToken cancellationToken = default)
    {
        var shift = await shiftRepository.GetByIdAsync(shiftId)
            ?? throw new ArgumentException("Shift not found.");
        if (shift.Staff.StaffId != requesterId)
        {
            throw new InvalidOperationException("You can only request a swap for your own shift.");
        }

        var requester = await staffRepository.GetByIdAsync(requesterId)
            ?? throw new ArgumentException("Requester not found.");
        var allStaff = await staffRepository.GetAllAsync();
        var allShifts = await shiftRepository.GetAllAsync();

        IEnumerable<Staff> candidates = requester switch
        {
            Doctor requestingDoctor => allStaff
                .OfType<Doctor>()
                .Where(doctor => doctor.StaffId != requesterId &&
                    string.Equals((doctor.Specialization ?? string.Empty).Trim(), (requestingDoctor.Specialization ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase))
                .Cast<Staff>(),
            Pharmacyst requestingPharmacyst => allStaff
                .OfType<Pharmacyst>()
                .Where(pharmacyst => pharmacyst.StaffId != requesterId &&
                    string.Equals((pharmacyst.Certification ?? string.Empty).Trim(), (requestingPharmacyst.Certification ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase))
                .Cast<Staff>(),
            _ => [],
        };

        return candidates
            .Where(candidate => !allShifts.Any(existingShift =>
                existingShift.Staff.StaffId == candidate.StaffId &&
                existingShift.StartTime < shift.EndTime &&
                existingShift.EndTime > shift.StartTime &&
                (existingShift.Status == ShiftStatus.Scheduled || existingShift.Status == ShiftStatus.Active)))
            .ToList();
    }

    public List<IStaff> GetEligibleSwapColleaguesForShift(int requesterId, int shiftId, out string error)
    {
        error = string.Empty;
        return GetEligibleSwapColleaguesAsync(requesterId, shiftId).GetAwaiter().GetResult().Cast<IStaff>().ToList();
    }

    public async Task<bool> AcceptSwapRequestAsync(int swapId, int colleagueId, CancellationToken cancellationToken = default)
    {
        var request = await shiftSwapRepository.GetByIdAsync(swapId);
        if (request?.Colleague?.StaffId != colleagueId || request.Shift is null)
        {
            return false;
        }

        request.Status = ShiftSwapRequestStatus.ACCEPTED;
        request.Shift.Staff = request.Colleague;
        await shiftRepository.UpdateAsync(request.Shift);
        await shiftSwapRepository.UpdateAsync(request);
        await notificationRepository.CreateAsync(new Notification
        {
            Recipient = request.Requester,
            Title = "Shift Swap Accepted",
            Message = $"Your swap request #{swapId} was accepted.",
            CreatedAt = DateTime.UtcNow,
        });
        return true;
    }

    public void AcceptSwapRequest(int swapId, int colleagueId, out string message)
    {
        message = AcceptSwapRequestAsync(swapId, colleagueId).GetAwaiter().GetResult()
            ? "Shift swap accepted."
            : "Shift swap could not be accepted.";
    }

    public async Task<bool> RejectSwapRequestAsync(int swapId, int colleagueId, CancellationToken cancellationToken = default)
    {
        var request = await shiftSwapRepository.GetByIdAsync(swapId);
        if (request?.Colleague?.StaffId != colleagueId)
        {
            return false;
        }

        request.Status = ShiftSwapRequestStatus.REJECTED;
        await shiftSwapRepository.UpdateAsync(request);
        await notificationRepository.CreateAsync(new Notification
        {
            Recipient = request.Requester,
            Title = "Shift Swap Rejected",
            Message = $"Your swap request #{swapId} was rejected.",
            CreatedAt = DateTime.UtcNow,
        });
        return true;
    }

    public void RejectSwapRequest(int swapId, int colleagueId, out string message)
    {
        message = RejectSwapRequestAsync(swapId, colleagueId).GetAwaiter().GetResult()
            ? "Shift swap rejected."
            : "Shift swap could not be rejected.";
    }

    public List<Doctor> GetAllDoctors() =>
        staffRepository.GetAllDoctorsAsync().GetAwaiter().GetResult();

    public List<ShiftSwapRequest> GetIncomingSwapRequests(int staffId) =>
        GetAllShiftSwapRequestsAsync().GetAwaiter().GetResult()
            .Where(request => request.Colleague?.StaffId == staffId && request.Status == ShiftSwapRequestStatus.PENDING)
            .ToList();
}

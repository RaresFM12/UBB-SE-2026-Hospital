using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Models.StaffPharmacy;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class ERDispatchService(
    IERDispatchRepository dispatchRepository,
    IStaffRepository staffRepository,
    IShiftRepository shiftRepository,
    INotificationRepository notificationRepository) : IERDispatchService
{
    private const string AssignedStatus = "ASSIGNED";
    private const string UnmatchedStatus = "UNMATCHED";
    private const string NotificationTitle = "ER Assignment";

    public async Task<IReadOnlyList<ERRequest>> GetAllRequestsAsync(CancellationToken cancellationToken = default)
        => await dispatchRepository.GetAllAsync();

    public async Task<ERRequest?> GetRequestByIdAsync(int requestId, CancellationToken cancellationToken = default)
        => await dispatchRepository.GetByIdAsync(requestId);

    public Task<ERRequest?> GetRequestByVisitIdAsync(int visitId)
    {
        // ERRequest currently has no visit relationship in the data model.
        return Task.FromResult<ERRequest?>(null);
    }

    public async Task<int> CreateRequestAsync(string specialization, string location, string status, CancellationToken cancellationToken = default)
    {
        var request = await dispatchRepository.CreateAsync(new ERRequest
        {
            Specialization = specialization,
            Location = location,
            Status = string.IsNullOrWhiteSpace(status) ? ERRequest.PendingStatus : status,
            CreatedAt = DateTime.UtcNow,
        });

        return request.Id;
    }

    public Task<int> CreateRequestAsync(string specialization, string location, CancellationToken cancellationToken = default) =>
        CreateRequestAsync(specialization, location, ERRequest.PendingStatus, cancellationToken);

    public async Task UpdateRequestStatusAsync(int requestId, string status, int? assignedDoctorId, string? assignedDoctorName, CancellationToken cancellationToken = default)
    {
        var request = await dispatchRepository.GetByIdAsync(requestId)
            ?? throw new ArgumentException("ER request not found.");

        request.Status = status;
        if (assignedDoctorId.HasValue)
        {
            request.AssignedDoctor = await staffRepository.GetByIdAsync(assignedDoctorId.Value) as Doctor;
        }

        await dispatchRepository.UpdateAsync(request);
    }

    public Task UpdateRequestStatusAsync(int requestId, string status, CancellationToken cancellationToken = default) =>
        UpdateRequestStatusAsync(requestId, status, null, null, cancellationToken);

    public async Task<IReadOnlyList<int>> GetPendingRequestIdsAsync(CancellationToken cancellationToken = default)
        => (await dispatchRepository.GetPendingAsync())
            .OrderBy(request => request.CreatedAt)
            .Select(request => request.Id)
            .ToList();

    public async Task<ERDispatchResult> DispatchERRequestAsync(int requestId, CancellationToken cancellationToken = default)
    {
        var request = await dispatchRepository.GetByIdAsync(requestId);
        if (request is null)
        {
            return new ERDispatchResult { RequestId = requestId, Success = false, Message = "ER request not found." };
        }

        var matchedDoctor = await FindBestMatchingDoctorAsync(request);
        if (matchedDoctor is null)
        {
            request.Status = UnmatchedStatus;
            await dispatchRepository.UpdateAsync(request);
            return new ERDispatchResult { RequestId = request.Id, Success = false, Message = "No available doctor matched the request." };
        }

        request.Status = AssignedStatus;
        request.AssignedDoctor = matchedDoctor;
        matchedDoctor.DoctorStatus = DoctorStatus.InExamination;

        await dispatchRepository.UpdateAsync(request);
        await staffRepository.UpdateAsync(matchedDoctor);
        await notificationRepository.CreateAsync(new Notification
        {
            Recipient = matchedDoctor,
            Title = NotificationTitle,
            Message = $"You have been assigned to ER request #{request.Id} ({request.Specialization}) at {request.Location}.",
            CreatedAt = DateTime.UtcNow,
        });

        return new ERDispatchResult
        {
            RequestId = request.Id,
            Success = true,
            AssignedDoctorId = matchedDoctor.StaffId,
            AssignedDoctorName = matchedDoctor.FullName,
            Message = $"Assigned to {matchedDoctor.FullName}.",
        };
    }

    public async Task<ERDispatchResult> ManualOverrideAsync(int requestId, int doctorId, int nearEndMinutes, CancellationToken cancellationToken = default)
    {
        var request = await dispatchRepository.GetByIdAsync(requestId)
            ?? throw new ArgumentException("ER request not found.");
        var doctor = await staffRepository.GetByIdAsync(doctorId) as Doctor
            ?? throw new ArgumentException("Doctor not found.");

        var shifts = await shiftRepository.GetByStaffIdAsync(doctorId);
        var now = DateTime.UtcNow;
        bool isNearEnd = shifts.Any(shift =>
            shift.Status == ShiftStatus.Active &&
            shift.EndTime >= now &&
            (shift.EndTime - now).TotalMinutes <= nearEndMinutes);

        if (!isNearEnd)
        {
            return new ERDispatchResult
            {
                RequestId = requestId,
                Success = false,
                Message = $"Manual override blocked. Doctor must be within {nearEndMinutes} minutes of ending their active shift.",
            };
        }

        request.Status = AssignedStatus;
        request.AssignedDoctor = doctor;
        doctor.DoctorStatus = DoctorStatus.InExamination;
        await dispatchRepository.UpdateAsync(request);
        await staffRepository.UpdateAsync(doctor);

        return new ERDispatchResult
        {
            RequestId = request.Id,
            Success = true,
            AssignedDoctorId = doctor.StaffId,
            AssignedDoctorName = doctor.FullName,
            Message = $"Manually assigned to {doctor.FullName}.",
        };
    }

    public async Task<IReadOnlyList<ERDispatchResult>> DispatchAllPendingAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<ERDispatchResult>();
        foreach (var requestId in await GetPendingRequestIdsAsync(cancellationToken))
        {
            results.Add(await DispatchERRequestAsync(requestId, cancellationToken));
        }

        return results;
    }

    public async Task<IReadOnlyList<int>> SimulateIncomingRequestsAsync(int count, CancellationToken cancellationToken = default)
    {
        var ids = new List<int>();
        for (int index = 0; index < count; index++)
        {
            ids.Add(await CreateRequestAsync("General", "ER", cancellationToken));
        }

        return ids;
    }

    public async Task<IReadOnlyList<DoctorProfile>> GetManualOverrideCandidatesAsync(int requestId, int nearEndMinutes, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var doctors = await staffRepository.GetAllDoctorsAsync();
        var shifts = await shiftRepository.GetAllAsync();

        return doctors
            .Where(doctor => shifts.Any(shift =>
                shift.Staff.StaffId == doctor.StaffId &&
                shift.Status == ShiftStatus.Active &&
                shift.EndTime >= now &&
                (shift.EndTime - now).TotalMinutes <= nearEndMinutes))
            .Select(doctor => new DoctorProfile
            {
                DoctorId = doctor.StaffId,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Specialization = doctor.Specialization,
                IsAvailable = true,
            })
            .ToList();
    }

    private async Task<Doctor?> FindBestMatchingDoctorAsync(ERRequest request)
    {
        var currentShifts = await shiftRepository.GetCurrentShiftsAsync();
        var doctors = await staffRepository.GetAllDoctorsAsync();

        return doctors
            .Where(doctor => doctor.DoctorStatus == DoctorStatus.Available)
            .Where(doctor => string.Equals(NormalizeSpecialization(doctor.Specialization), NormalizeSpecialization(request.Specialization), StringComparison.OrdinalIgnoreCase))
            .Where(doctor => currentShifts.Any(shift => shift.Staff.StaffId == doctor.StaffId && string.Equals(shift.Location, request.Location, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(doctor => doctor.FullName)
            .FirstOrDefault();
    }

    private static string NormalizeSpecialization(string specialization)
        => (specialization ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "surgeon" => "surgery",
            "cardiologist" => "cardiology",
            "cardio" => "cardiology",
            "pediatric" => "pediatrics",
            "pediatrician" => "pediatrics",
            _ => (specialization ?? string.Empty).Trim().ToLowerInvariant(),
        };
}

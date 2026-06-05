using Hospital.Data.Models;
using Hospital.Shared.Models.StaffPharmacy;

namespace Hospital.Shared.Services;

public interface IERDispatchService
{
    Task<IReadOnlyList<ERRequest>> GetAllRequestsAsync(CancellationToken cancellationToken = default);
    Task<ERRequest?> GetRequestByIdAsync(int requestId, CancellationToken cancellationToken = default);
    Task<int> CreateRequestAsync(string specialization, string location, string status, CancellationToken cancellationToken = default);
    Task<int> CreateRequestAsync(string specialization, string location, CancellationToken cancellationToken = default);
    Task UpdateRequestStatusAsync(int requestId, string status, int? assignedDoctorId, string? assignedDoctorName, CancellationToken cancellationToken = default);
    Task UpdateRequestStatusAsync(int requestId, string status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetPendingRequestIdsAsync(CancellationToken cancellationToken = default);
    Task<ERDispatchResult> DispatchERRequestAsync(int requestId, CancellationToken cancellationToken = default);
    Task<ERDispatchResult> ManualOverrideAsync(int requestId, int doctorId, int nearEndMinutes, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ERDispatchResult>> DispatchAllPendingAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> SimulateIncomingRequestsAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DoctorProfile>> GetManualOverrideCandidatesAsync(int requestId, int nearEndMinutes, CancellationToken cancellationToken = default);
    Task<Hospital.Data.Models.ERRequest?> GetRequestByVisitIdAsync(int visitId);
}

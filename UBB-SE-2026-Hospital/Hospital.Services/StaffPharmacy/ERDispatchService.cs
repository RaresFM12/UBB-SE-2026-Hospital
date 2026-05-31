using Hospital.Data.Models;
using Hospital.Shared.Models.StaffPharmacy;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class ERDispatchService : IERDispatchService
{
    public Task<IReadOnlyList<ERRequest>> GetAllRequestsAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<ERRequest?> GetRequestByIdAsync(int requestId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<int> CreateRequestAsync(string specialization, string location, string status, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UpdateRequestStatusAsync(int requestId, string status, int? assignedDoctorId, string? assignedDoctorName, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<int>> GetPendingRequestIdsAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<ERDispatchResult> DispatchERRequestAsync(int requestId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<ERDispatchResult> ManualOverrideAsync(int requestId, int doctorId, int nearEndMinutes, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<ERDispatchResult>> DispatchAllPendingAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}

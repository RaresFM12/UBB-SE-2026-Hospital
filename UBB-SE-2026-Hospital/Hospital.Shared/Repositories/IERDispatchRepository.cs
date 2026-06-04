namespace Hospital.Shared.Repositories
{
    using System.Collections.Generic;
    using Hospital.Shared.Models;

    public interface IERDispatchRepository
    {
        int AddRequest(string specialization, string location, string status);

        IReadOnlyList<ERRequest> GetAllRequests();

        ERRequest? GetRequestById(int requestId);

        void UpdateRequestStatus(int requestId, string status, int? assignedDoctorId, string? assignedDoctorName);
    }
}

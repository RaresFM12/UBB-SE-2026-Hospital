using System;
using System.Collections.Generic;
using System.Text;
using Hospital.Data.Models;
namespace Hospital.Shared.Proxies;

public interface ITransplantApiClient
{
    Task<Transplant?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Transplant>> GetByReceiverIdAsync(int receiverId, CancellationToken cancellationToken = default);
    Task<List<Transplant>> GetByDonorIdAsync(int donorId, CancellationToken cancellationToken = default);
    Task<List<TransplantMatch>> GetTopMatchesForDonorAsync(int donorId, string organType, CancellationToken cancellationToken = default);
    Task<bool> IsUrgentAsync(int patientId, CancellationToken cancellationToken = default);
    Task<string?> GetChronicWarningAsync(int patientId, CancellationToken cancellationToken = default);
    Task CreateWaitlistRequestAsync(int receiverId, string organType, CancellationToken cancellationToken = default);
    Task AssignDonorAsync(int transplantId, int donorId, float finalScore, CancellationToken cancellationToken = default);
}


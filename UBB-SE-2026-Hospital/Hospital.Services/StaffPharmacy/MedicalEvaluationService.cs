using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class MedicalEvaluationService : IMedicalEvaluationService
{
    public Task<IReadOnlyList<MedicalEvaluation>> GetAllEvaluationsAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<MedicalEvaluation>> GetEvaluationsByDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<MedicalEvaluation?> GetEvaluationByIdAsync(int evaluationId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task CreateEvaluationAsync(int doctorId, int patientId, string diagnosis, string notes, string medications, bool assumedRisk, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UpdateEvaluationAsync(int evaluationId, string diagnosis, string notes, string medications, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteEvaluationAsync(int evaluationId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> IsDoctorFatiguedAsync(int doctorId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<string?> CheckMedicineConflictAsync(int patientId, string medications, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}

using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IMedicalEvaluationService
{
    Task<IReadOnlyList<MedicalEvaluation>> GetAllEvaluationsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MedicalEvaluation>> GetEvaluationsByDoctorAsync(int doctorId, CancellationToken cancellationToken = default);

    Task<MedicalEvaluation?> GetEvaluationByIdAsync(int evaluationId, CancellationToken cancellationToken = default);

    Task CreateEvaluationAsync(int doctorId, int patientId, string diagnosis, string notes, string medications, bool assumedRisk, CancellationToken cancellationToken = default);

    Task UpdateEvaluationAsync(int evaluationId, string diagnosis, string notes, string medications, CancellationToken cancellationToken = default);

    Task DeleteEvaluationAsync(int evaluationId, CancellationToken cancellationToken = default);

    Task<bool> IsDoctorFatiguedAsync(int doctorId, CancellationToken cancellationToken = default);

    Task<string?> CheckMedicineConflictAsync(int patientId, string medications, CancellationToken cancellationToken = default);
}

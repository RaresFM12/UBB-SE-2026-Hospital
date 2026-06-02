using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IMedicalEvaluationService
{
    Task<IReadOnlyList<MedicalEvaluation>> GetAllEvaluationsAsync(CancellationToken cancellationToken = default);
    IReadOnlyList<MedicalEvaluation> GetAllEvaluations();
    Task<IReadOnlyList<MedicalEvaluation>> GetEvaluationsByDoctorAsync(int doctorId, CancellationToken cancellationToken = default);
    IReadOnlyList<MedicalEvaluation> GetEvaluationsByDoctor(string doctorId);
    Task<MedicalEvaluation?> GetEvaluationByIdAsync(int evaluationId, CancellationToken cancellationToken = default);
    MedicalEvaluation? GetEvaluationById(int evaluationId);
    Task CreateEvaluationAsync(int doctorId, int patientId, string diagnosis, string notes, string medications, bool assumedRisk, CancellationToken cancellationToken = default);
    void SaveEvaluation(MedicalEvaluation evaluation);
    Task UpdateEvaluationAsync(int evaluationId, string diagnosis, string notes, string medications, CancellationToken cancellationToken = default);
    void UpdateEvaluation(MedicalEvaluation evaluation);
    Task DeleteEvaluationAsync(int evaluationId, CancellationToken cancellationToken = default);
    void DeleteEvaluation(int evaluationId);
    Task<bool> IsDoctorFatiguedAsync(int doctorId, CancellationToken cancellationToken = default);
    bool IsDoctorFatigued(string doctorId);
    Task<string?> CheckMedicineConflictAsync(int patientId, string medications, CancellationToken cancellationToken = default);
    string? CheckMedicineConflict(string patientId, string medications);
    IReadOnlyList<Doctor> GetAllDoctors();
}

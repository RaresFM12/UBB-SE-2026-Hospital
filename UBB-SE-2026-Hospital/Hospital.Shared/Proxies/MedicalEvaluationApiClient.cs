using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class MedicalEvaluationApiClient : ApiClientBase, IMedicalEvaluationService
{
    private const string BaseUri = "api/evaluations";

    public MedicalEvaluationApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<MedicalEvaluation>> GetAllEvaluationsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<MedicalEvaluation>>(BaseUri, cancellationToken) ?? new List<MedicalEvaluation>();

    public Task CreateEvaluationAsync(int doctorId, int patientId, string diagnosis, string notes, string medications, bool assumedRisk, CancellationToken cancellationToken = default)
        => PostAsync(BaseUri, new
        {
            DoctorId = doctorId,
            PatientId = patientId,
            Diagnosis = diagnosis,
            Notes = notes,
            Medications = medications,
            AssumedRisk = assumedRisk,
        }, cancellationToken);

    public Task UpdateEvaluationAsync(int evaluationId, string diagnosis, string notes, string medications, CancellationToken cancellationToken = default)
        => PutAsync($"{BaseUri}/{evaluationId}", new
        {
            Diagnosis = diagnosis,
            Notes = notes,
            Medications = medications,
        }, cancellationToken);

    public Task DeleteEvaluationAsync(int evaluationId, CancellationToken cancellationToken = default)
        => DeleteAsync($"{BaseUri}/{evaluationId}", cancellationToken);

    // Members without a usable endpoint in the desktop client.
    public IReadOnlyList<MedicalEvaluation> GetAllEvaluations()
        => throw new NotSupportedException("Not available in the desktop client.");

    public Task<IReadOnlyList<MedicalEvaluation>> GetEvaluationsByDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
        => Task.FromException<IReadOnlyList<MedicalEvaluation>>(new NotSupportedException("Not available in the desktop client."));

    public IReadOnlyList<MedicalEvaluation> GetEvaluationsByDoctor(string doctorId)
        => throw new NotSupportedException("Not available in the desktop client.");

    public Task<MedicalEvaluation?> GetEvaluationByIdAsync(int evaluationId, CancellationToken cancellationToken = default)
        => Task.FromException<MedicalEvaluation?>(new NotSupportedException("Not available in the desktop client."));

    public MedicalEvaluation? GetEvaluationById(int evaluationId)
        => throw new NotSupportedException("Not available in the desktop client.");

    public void SaveEvaluation(MedicalEvaluation evaluation)
        => throw new NotSupportedException("Not available in the desktop client.");

    public void UpdateEvaluation(MedicalEvaluation evaluation)
        => throw new NotSupportedException("Not available in the desktop client.");

    public void DeleteEvaluation(int evaluationId)
        => throw new NotSupportedException("Not available in the desktop client.");

    public Task<bool> IsDoctorFatiguedAsync(int doctorId, CancellationToken cancellationToken = default)
        => Task.FromException<bool>(new NotSupportedException("Not available in the desktop client."));

    public bool IsDoctorFatigued(string doctorId)
        => throw new NotSupportedException("Not available in the desktop client.");

    public Task<string?> CheckMedicineConflictAsync(int patientId, string medications, CancellationToken cancellationToken = default)
        => Task.FromException<string?>(new NotSupportedException("Not available in the desktop client."));

    public string? CheckMedicineConflict(string patientId, string medications)
        => throw new NotSupportedException("Not available in the desktop client.");

    public IReadOnlyList<Doctor> GetAllDoctors()
        => throw new NotSupportedException("Not available in the desktop client.");
}

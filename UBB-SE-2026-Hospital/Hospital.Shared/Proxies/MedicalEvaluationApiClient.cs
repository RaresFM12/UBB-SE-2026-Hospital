using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class MedicalEvaluationApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IMedicalEvaluationService, IMedicalEvaluationApiClient
{
    private const string BaseUri = "api/evaluations";
    private const string DoctorsUri = "api/staff/doctors";

    public async Task<IReadOnlyList<MedicalEvaluation>> GetAllEvaluationsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<MedicalEvaluation>>(BaseUri, cancellationToken) ?? [];

    public IReadOnlyList<MedicalEvaluation> GetAllEvaluations()
        => Task.Run(async () => await GetAsync<List<MedicalEvaluation>>(BaseUri) ?? []).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<MedicalEvaluation>> GetEvaluationsByDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<MedicalEvaluation> allEvaluations = await GetAllEvaluationsAsync(cancellationToken);
        return allEvaluations.Where(evaluation => evaluation.Evaluator?.StaffId == doctorId).ToList();
    }

    public IReadOnlyList<MedicalEvaluation> GetEvaluationsByDoctor(string doctorId)
        => GetAllEvaluations().Where(evaluation => (evaluation.Evaluator?.StaffId.ToString() ?? string.Empty) == doctorId).ToList();

    public async Task<MedicalEvaluation?> GetEvaluationByIdAsync(int evaluationId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<MedicalEvaluation> allEvaluations = await GetAllEvaluationsAsync(cancellationToken);
        return allEvaluations.FirstOrDefault(evaluation => evaluation.EvaluationID == evaluationId);
    }

    public MedicalEvaluation? GetEvaluationById(int evaluationId)
        => GetAllEvaluations().FirstOrDefault(evaluation => evaluation.EvaluationID == evaluationId);

    public async Task CreateEvaluationAsync(int doctorId, int patientId, string diagnosis, string notes, string medications, bool assumedRisk, CancellationToken cancellationToken = default)
        => await PostAsync(BaseUri, new { doctorId, patientId, diagnosis, notes, medications, assumedRisk }, cancellationToken);

    public void SaveEvaluation(MedicalEvaluation evaluation)
        => Task.Run(async () => await CreateEvaluationAsync(
            evaluation.Evaluator?.StaffId ?? 0,
            int.TryParse(evaluation.PatientId, out int parsedPatientId) ? parsedPatientId : 0,
            evaluation.Symptoms,
            evaluation.Notes,
            evaluation.MedicationsList,
            false)).GetAwaiter().GetResult();

    public async Task UpdateEvaluationAsync(int evaluationId, string diagnosis, string notes, string medications, CancellationToken cancellationToken = default)
        => await PutAsync($"{BaseUri}/{evaluationId}", new { diagnosis, notes, medications }, cancellationToken);

    public void UpdateEvaluation(MedicalEvaluation evaluation)
        => Task.Run(async () => await UpdateEvaluationAsync(
            evaluation.EvaluationID,
            evaluation.Symptoms,
            evaluation.Notes,
            evaluation.MedicationsList)).GetAwaiter().GetResult();

    public async Task DeleteEvaluationAsync(int evaluationId, CancellationToken cancellationToken = default)
        => await DeleteAsync($"{BaseUri}/{evaluationId}", cancellationToken);

    public void DeleteEvaluation(int evaluationId)
        => Task.Run(async () => await DeleteEvaluationAsync(evaluationId)).GetAwaiter().GetResult();

    // No dedicated API endpoint: fatigue checks are evaluated server-side at creation time.
    public Task<bool> IsDoctorFatiguedAsync(int doctorId, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public bool IsDoctorFatigued(string doctorId) => false;

    // No dedicated API endpoint: conflict validation is enforced by the API on create/update.
    public Task<string?> CheckMedicineConflictAsync(int patientId, string medications, CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(null);

    public string? CheckMedicineConflict(string patientId, string medications) => null;

    public IReadOnlyList<Doctor> GetAllDoctors()
        => Task.Run(async () =>
        {
            List<DoctorSummaryDto> doctorSummaries = await GetAsync<List<DoctorSummaryDto>>(DoctorsUri) ?? [];
            return doctorSummaries
                .Select(doctorSummary => new Doctor { StaffId = doctorSummary.StaffId, FirstName = doctorSummary.FirstName, LastName = doctorSummary.LastName })
                .ToList();
        }).GetAwaiter().GetResult();

    private sealed class DoctorSummaryDto
    {
        public int StaffId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}

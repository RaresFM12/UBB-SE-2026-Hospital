#if false
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class MedicalEvaluationService(
    IEvaluationsRepository evaluationsRepository,
    IHighRiskMedicineRepository highRiskMedicineRepository,
    IShiftRepository shiftRepository) : IMedicalEvaluationService
{
    private const double FatigueThresholdHours = 12.0;
    private const double FatigueLookbackHours = 24.0;
    private static readonly char[] MedicationSeparators = [',', ';', '\n', '\r', '/', '|'];

    public async Task<IReadOnlyList<MedicalEvaluation>> GetAllEvaluationsAsync(CancellationToken cancellationToken = default)
        => await evaluationsRepository.GetAllAsync();

    public async Task<IReadOnlyList<MedicalEvaluation>> GetEvaluationsByDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
        => await evaluationsRepository.GetByDoctorIdAsync(doctorId);

    public async Task<MedicalEvaluation?> GetEvaluationByIdAsync(int evaluationId, CancellationToken cancellationToken = default)
        => await evaluationsRepository.GetByIdAsync(evaluationId);

    public async Task CreateEvaluationAsync(int doctorId, int patientId, string diagnosis, string notes, string medications, bool assumedRisk, CancellationToken cancellationToken = default)
    {
        await evaluationsRepository.CreateAsync(new MedicalEvaluation
        {
            Evaluator = new Doctor { StaffId = doctorId },
            PatientId = patientId.ToString(),
            Symptoms = assumedRisk ? $"{diagnosis} [RISK]" : diagnosis,
            Notes = notes,
            MedicationsList = medications,
            EvaluationDate = DateTime.UtcNow,
        });
    }

    public async Task UpdateEvaluationAsync(int evaluationId, string diagnosis, string notes, string medications, CancellationToken cancellationToken = default)
    {
        var evaluation = await evaluationsRepository.GetByIdAsync(evaluationId)
            ?? throw new ArgumentException("Evaluation not found.");

        evaluation.Symptoms = diagnosis;
        evaluation.Notes = notes;
        evaluation.MedicationsList = medications;
        await evaluationsRepository.UpdateAsync(evaluation);
    }

    public async Task DeleteEvaluationAsync(int evaluationId, CancellationToken cancellationToken = default)
        => await evaluationsRepository.DeleteAsync(evaluationId);

    public async Task<bool> IsDoctorFatiguedAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        DateTime lookbackStart = DateTime.UtcNow.AddHours(-FatigueLookbackHours);
        double recentHours = (await shiftRepository.GetByStaffIdAsync(doctorId))
            .Where(shift => shift.EndTime >= lookbackStart)
            .Sum(shift => (shift.EndTime - shift.StartTime).TotalHours);

        return recentHours >= FatigueThresholdHours;
    }

    public async Task<string?> CheckMedicineConflictAsync(int patientId, string medications, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(medications))
        {
            return null;
        }

        var medicineName = medications.Trim();
        var highRiskMedicine = (await highRiskMedicineRepository.GetAllAsync())
            .FirstOrDefault(medicine => string.Equals(medicine.MedicineName, medicineName, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(highRiskMedicine?.WarningMessage))
        {
            return highRiskMedicine.WarningMessage;
        }

        var currentDrugs = SplitMedicines(medications);
        foreach (var evaluation in await evaluationsRepository.GetAllAsync())
        {
            if (!string.Equals(evaluation.PatientId, patientId.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!ContainsKeyword(evaluation.Symptoms, "Allergy") &&
                !ContainsKeyword(evaluation.Symptoms, "Adverse") &&
                !ContainsKeyword(evaluation.Notes, "Allergy") &&
                !ContainsKeyword(evaluation.Notes, "Adverse"))
            {
                continue;
            }

            var historicalDrugs = SplitMedicines(evaluation.MedicationsList);
            var sharedDrug = currentDrugs.FirstOrDefault(drug => historicalDrugs.Any(hist => string.Equals(hist, drug, StringComparison.OrdinalIgnoreCase)));
            if (sharedDrug is not null)
            {
                return $"HISTORY ALERT: Patient had a past Adverse Reaction/Allergy to {sharedDrug} recorded in their history.";
            }
        }

        return null;
    }

    private static List<string> SplitMedicines(string medications)
        => medications
            .Split(MedicationSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(token => token.Trim())
            .Where(token => token.Length > 0)
            .ToList();

    private static bool ContainsKeyword(string? text, string keyword)
        => !string.IsNullOrWhiteSpace(text) && text.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    public List<Doctor> GetAllDoctors() { throw new System.NotImplementedException(); }
    public List<Appointment> GetAppointmentsByDoctor(int doctorId) { throw new System.NotImplementedException(); }
    public List<MedicalEvaluation> GetEvaluationsByDoctor(string doctorId) { throw new System.NotImplementedException(); }
    public void SaveEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void UpdateEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void DeleteEvaluation(int evaluationId) { throw new System.NotImplementedException(); }
    public bool IsDoctorFatigued(string doctorId) { throw new System.NotImplementedException(); }
    public void RaiseFatigueIntervention(int doctorId, string doctorName) { throw new System.NotImplementedException(); }
    public string? CheckMedicineConflict(string patientId, string medications) { throw new System.NotImplementedException(); }
    public MedicalEvaluation? GetEvaluationById(int evaluationId) { throw new System.NotImplementedException(); }
    public void SaveEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void UpdateEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void SaveEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void UpdateEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void SaveEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void UpdateEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void SaveEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void UpdateEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void SaveEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void UpdateEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void SaveEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void UpdateEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void SaveEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void UpdateEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void SaveEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void UpdateEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void SaveEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
    public void UpdateEvaluation(MedicalEvaluation record) { throw new System.NotImplementedException(); }
}
#endif

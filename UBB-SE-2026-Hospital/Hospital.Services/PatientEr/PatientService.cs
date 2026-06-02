using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.PatientEr;

public class PatientService(
    IPatientRepository patientRepository,
    IMedicalHistoryRepository medicalHistoryRepository,
    IMedicalRecordRepository medicalRecordRepository,
    IPrescriptionRepository prescriptionRepository) : IPatientService
{
    public async Task<IReadOnlyList<Patient>> GetPatientsAsync(CancellationToken cancellationToken = default)
        => await patientRepository.GetAllAsync();

    public Task<Patient?> GetByIdAsync(int id)
        => patientRepository.GetByIdAsync(id);

    public async Task<Patient> GetPatientDetailsAsync(int id)
    {
        return await patientRepository.GetByIdAsync(id)
            ?? throw new ArgumentException($"Patient {id} was not found.");
    }

    public Task<MedicalHistory?> GetMedicalHistoryAsync(int id)
        => medicalHistoryRepository.GetByPatientIdAsync(id);

    public async Task<List<MedicalRecord>> GetMedicalRecordsAsync(int historyId)
        => await medicalRecordRepository.GetByMedicalHistoryIdAsync(historyId);

    public async Task<int> CreateMedicalRecordAsync(int patientId, CreateMedicalRecordRequest dto)
    {
        MedicalHistory history = await medicalHistoryRepository.GetByPatientIdAsync(patientId)
            ?? throw new ArgumentException($"Medical history for patient {patientId} was not found.");

        var record = new MedicalRecord
        {
            MedicalHistory = history,
            SourceType = dto.SourceType,
            SourceId = dto.SourceId,
            Symptoms = dto.Symptoms,
            Diagnosis = dto.Diagnosis,
            ConsultationDate = dto.ConsultationDate == default ? DateTime.Now : dto.ConsultationDate,
            BasePrice = dto.BasePrice,
            FinalPrice = dto.FinalPrice,
            PoliceNotified = dto.PoliceNotified,
        };

        MedicalRecord created = await medicalRecordRepository.CreateAsync(record);
        return created.RecordId;
    }

    public async Task<List<string>> GetPatientAllergiesAsync(int id)
    {
        MedicalHistory? history = await medicalHistoryRepository.GetByPatientIdAsync(id);
        return history?.PatientAllergies
            .Where(pa => pa.Allergy is not null)
            .Select(pa => pa.Allergy.AllergyName)
            .ToList() ?? new List<string>();
    }

    public async Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId)
        => (await prescriptionRepository.GetByRecordIdAsync(recordId)).FirstOrDefault();

    public async Task<bool> IsHighRiskPatientAsync(int id)
    {
        MedicalHistory? history = await medicalHistoryRepository.GetByPatientIdAsync(id);
        if (history is null)
        {
            return false;
        }

        List<MedicalRecord> records = await medicalRecordRepository.GetByMedicalHistoryIdAsync(history.MedicalHistoryId);
        return records.Any(r => r.PoliceNotified);
    }

    public async Task<bool> ExistsAsync(string cnp)
        => (await patientRepository.GetAllAsync()).Any(p => p.Cnp == cnp);

    public async Task<List<Patient>> SearchPatientsAsync(SearchPatientsRequest dto)
        => await patientRepository.GetFilteredAsync(new PatientFilter
        {
            CNP = dto.Cnp,
            NamePart = dto.NamePart,
            MinAge = dto.MinAge,
            MaxAge = dto.MaxAge,
            Sex = dto.Sex,
        });

    public async Task<Patient> CreatePatientAsync(CreatePatientRequest dto)
        => await patientRepository.CreateAsync(new Patient
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Cnp = dto.Cnp,
            DateOfBirth = dto.DateOfBirth,
            Sex = dto.Sex,
            PhoneNumber = dto.PhoneNumber,
            EmergencyContact = dto.EmergencyContact,
            IsDonor = dto.IsDonor,
        });

    public async Task UpdatePatientAsync(int id, UpdatePatientRequest dto)
    {
        Patient patient = await patientRepository.GetByIdAsync(id)
            ?? throw new ArgumentException($"Patient {id} was not found.");

        patient.FirstName = dto.FirstName;
        patient.LastName = dto.LastName;
        patient.Cnp = dto.Cnp;
        patient.DateOfBirth = dto.DateOfBirth;
        patient.Sex = dto.Sex;
        patient.PhoneNumber = dto.PhoneNumber;
        patient.EmergencyContact = dto.EmergencyContact;
        patient.IsDonor = dto.IsDonor;
        patient.Transferred = dto.Transferred;
        patient.DateOfDeath = dto.DateOfDeath;
        patient.IsArchived = dto.IsArchived;

        await patientRepository.UpdateAsync(patient);
    }

    public async Task ArchivePatientAsync(int id)
    {
        Patient patient = await patientRepository.GetByIdAsync(id)
            ?? throw new ArgumentException($"Patient {id} was not found.");
        patient.IsArchived = true;
        await patientRepository.UpdateAsync(patient);
    }

    public async Task DearchivePatientAsync(int id)
    {
        Patient patient = await patientRepository.GetByIdAsync(id)
            ?? throw new ArgumentException($"Patient {id} was not found.");
        patient.IsArchived = false;
        await patientRepository.UpdateAsync(patient);
    }

    public async Task ArchiveAsDeceasedAsync(int id, ArchiveAsDeceasedRequest dto)
    {
        Patient patient = await patientRepository.GetByIdAsync(id)
            ?? throw new ArgumentException($"Patient {id} was not found.");
        patient.DateOfDeath = dto.DeathDate;
        patient.IsArchived = true;
        await patientRepository.UpdateAsync(patient);
    }

    public async Task CreateMedicalHistoryAsync(int id, CreateMedicalHistoryRequest dto)
    {
        Patient patient = await patientRepository.GetByIdAsync(id)
            ?? throw new ArgumentException($"Patient {id} was not found.");

        await medicalHistoryRepository.CreateAsync(new MedicalHistory
        {
            Patient = patient,
            BloodType = dto.BloodType,
            Rh = dto.Rh,
            ChronicConditions = dto.ChronicConditions,
        });
    }

    public Task DeletePatientAsync(int id)
        => patientRepository.DeleteAsync(id);
}

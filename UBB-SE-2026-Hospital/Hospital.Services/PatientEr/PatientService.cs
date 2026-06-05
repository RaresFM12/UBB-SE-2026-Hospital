using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;
using DbPatient = Hospital.Data.Models.Patient;

namespace Hospital.Services.PatientEr;

public class PatientService(
    IPatientRepository patientRepository,
    IPrescriptionRepository? prescriptionRepository = null,
    IAllergyRepository? allergyRepository = null) : IPatientService
{
    private const int HighRiskRecordThreshold = 3;

    public Task<List<Data.Models.Patient>> SearchPatientsAsync(SearchPatientsRequest? searchCriteria, CancellationToken cancellationToken)
    {
        if (searchCriteria is null)
        {
            return patientRepository.GetAllAsync();
        }

        var filter = new PatientFilter
        {
            NamePart = searchCriteria.NamePart,
            CNP = searchCriteria.Cnp,
            MinAge = searchCriteria.MinAge,
            MaxAge = searchCriteria.MaxAge,
            Sex = searchCriteria.Sex,
            HasChronicCondition = searchCriteria.HasChronicCondition,
            LastUpdatedFrom = searchCriteria.LastUpdatedFrom,
            LastUpdatedTo = searchCriteria.LastUpdatedTo,
            BloodType = searchCriteria.BloodType,
            Rh = searchCriteria.Rh,
        };

        return patientRepository.GetFilteredAsync(filter);
    }

    public Task<Data.Models.Patient?> GetByIdAsync(int id, CancellationToken cancellationToken) => patientRepository.GetByIdAsync(id);

    public async Task<IReadOnlyList<DbPatient>> GetPatientsAsync(CancellationToken cancellationToken = default)
        => await patientRepository.GetAllAsync();

    public async Task<DbPatient> CreatePatientAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingPatients = await patientRepository.GetAllAsync();
        if (existingPatients.Any(p => string.Equals(p.Cnp, request.Cnp, StringComparison.Ordinal)))
            throw new ArgumentException("A patient with this CNP already exists.");

        var patient = new DbPatient
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Cnp = request.Cnp.Trim(),
            DateOfBirth = request.DateOfBirth,
            Sex = request.Sex,
            PhoneNumber = request.PhoneNumber.Trim(),
            EmergencyContact = request.EmergencyContact.Trim(),
            IsArchived = false,
            IsDonor = request.IsDonor,
            Transferred = false
        };

        if (!patient.Validate(out List<string> errors))
            throw new ArgumentException(string.Join(" ", errors));

        return await patientRepository.CreateAsync(patient);
    }

    public async Task CreateMedicalHistoryAsync(int patientId, CreateMedicalHistoryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var patient = await patientRepository.GetByIdAsync(patientId)
            ?? throw new ArgumentException("Patient not found.");

        if (patient.MedicalHistory is not null)
            throw new ArgumentException("Patient already has a medical history.");

        var medicalHistory = new Data.Models.MedicalHistory
        {
            Patient = patient,
            BloodType = request.BloodType,
            Rh = request.Rh,
            ChronicConditions = request.ChronicConditions
                .Where(condition => !string.IsNullOrWhiteSpace(condition))
                .Select(condition => condition.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
        };

        if (request.AllergyIds.Count > 0)
        {
            if (allergyRepository is null)
                throw new InvalidOperationException("Allergy repository is not available.");

            foreach (int allergyId in request.AllergyIds.Distinct())
            {
                var allergy = await allergyRepository.GetByIdAsync(allergyId)
                    ?? throw new ArgumentException($"Allergy with ID {allergyId} was not found.");

                medicalHistory.PatientAllergies.Add(new Data.Models.PatientAllergy
                {
                    MedicalHistory = medicalHistory,
                    Allergy = allergy,
                    AllergyId = allergy.AllergyId,
                    SeverityLevel = "Mild"
                });
            }
        }

        patient.MedicalHistory = medicalHistory;
        await patientRepository.UpdateAsync(patient);
    }

    public async Task<DbPatient> GetPatientDetailsAsync(int patientId, CancellationToken cancellationToken)
        => await patientRepository.GetByIdAsync(patientId)
            ?? throw new ArgumentException("Patient not found.");

    public async Task<Hospital.Data.Models.Prescription?> GetPrescriptionByRecordIdAsync(int recordId, CancellationToken cancellationToken)
    {
        if (prescriptionRepository is null)
            return null;

        return (await prescriptionRepository.GetByRecordIdAsync(recordId)).FirstOrDefault();
    }

    public async Task<List<string>> GetPatientAllergiesAsync(int patientId, CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdAsync(patientId)
            ?? throw new ArgumentException("Patient not found.");
        return patient.MedicalHistory?.PatientAllergies
            .Where(patientAllergy => patientAllergy.Allergy is not null)
            .Select(patientAllergy => patientAllergy.Allergy.AllergyName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(allergyName => allergyName)
            .ToList() ?? [];
    }

    public async Task<bool> IsHighRiskPatientAsync(int patientId, CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdAsync(patientId)
            ?? throw new ArgumentException("Patient not found.");
        var records = patient.MedicalHistory?.MedicalRecords ?? [];
        return records.Count(record => record.PoliceNotified) > 0 ||
            records.Count >= HighRiskRecordThreshold ||
            patient.MedicalHistory?.ChronicConditions.Count > 0;
    }

    public async Task<RecordExportDataDto> GetRecordExportDataAsync(int recordId, CancellationToken cancellationToken)
    {
        var patients = await patientRepository.GetAllAsync();
        foreach (var patient in patients)
        {
            var detailedPatient = await patientRepository.GetByIdAsync(patient.PatientId);
            var record = detailedPatient?.MedicalHistory?.MedicalRecords.FirstOrDefault(record => record.RecordId == recordId);
            if (detailedPatient is not null && record is not null)
            {
                var prescription = record.Prescription;
                return new RecordExportDataDto
                {
                    Patient = detailedPatient,
                    Record = record,
                    Prescription = prescription,
                    Items = prescription?.MedicationList ?? [],
                };
            }
        }

        throw new ArgumentException("Medical record not found.");
    }

    public async Task UpdatePatientAsync(DbPatient patient, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(patient);

        var existingPatient = await patientRepository.GetByIdAsync(patient.PatientId)
            ?? throw new ArgumentException("Patient not found.");

        existingPatient.FirstName = patient.FirstName;
        existingPatient.LastName = patient.LastName;
        existingPatient.Cnp = patient.Cnp;
        existingPatient.DateOfBirth = patient.DateOfBirth;
        existingPatient.DateOfDeath = patient.DateOfDeath;
        existingPatient.Sex = patient.Sex;
        existingPatient.PhoneNumber = patient.PhoneNumber;
        existingPatient.EmergencyContact = patient.EmergencyContact;
        existingPatient.IsArchived = patient.IsArchived;
        existingPatient.IsDonor = patient.IsDonor;
        existingPatient.Transferred = patient.Transferred;

        if (!existingPatient.Validate(out List<string> errors))
            throw new ArgumentException(string.Join(" ", errors));

        if (existingPatient.DateOfDeath.HasValue && existingPatient.DateOfDeath.Value.Date > DateTime.Today)
            throw new ArgumentException("Date of death cannot be in the future.");

        await patientRepository.UpdateAsync(existingPatient);
    }

    public async Task ArchivePatientAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var patient = await patientRepository.GetByIdAsync(patientId)
            ?? throw new ArgumentException("Patient not found.");

        patient.IsArchived = true;
        await patientRepository.UpdateAsync(patient);
    }

    public async Task DearchivePatientAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var patient = await patientRepository.GetByIdAsync(patientId)
            ?? throw new ArgumentException("Patient not found.");

        if (patient.IsDeceased)
            throw new ArgumentException("Deceased patients cannot be restored to active status.");

        patient.IsArchived = false;
        await patientRepository.UpdateAsync(patient);
    }

    public async Task ArchiveAsDeceasedAsync(int patientId, DateTime deathDate, CancellationToken cancellationToken = default)
    {
        if (deathDate.Date > DateTime.Today)
            throw new ArgumentException("Date of death cannot be in the future.");

        var patient = await patientRepository.GetByIdAsync(patientId)
            ?? throw new ArgumentException("Patient not found.");

        if (deathDate.Date < patient.DateOfBirth.Date)
            throw new ArgumentException("Date of death cannot be before date of birth.");

        patient.IsArchived = true;
        patient.DateOfDeath = deathDate.Date;
        await patientRepository.UpdateAsync(patient);
    }

    public async Task<int> CreateMedicalRecordAsync(int patientId, Data.Models.MedicalRecord record, CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdAsync(patientId)
            ?? throw new ArgumentException("Patient not found.");
        if (patient.MedicalHistory is null)
        {
            patient.MedicalHistory = new Data.Models.MedicalHistory { Patient = patient };
        }

        record.MedicalHistory = patient.MedicalHistory;
        patient.MedicalHistory.MedicalRecords.Add(record);
        await patientRepository.UpdateAsync(patient);
        return record.RecordId;
    }

    public async Task CreatePrescriptionAsync(int recordId, Hospital.Data.Models.Prescription prescription)
    {
        if (prescriptionRepository is null)
            throw new InvalidOperationException("Prescription repository is not available.");

        var exportData = await GetRecordExportDataAsync(recordId, CancellationToken.None);
        prescription.MedicalRecord = exportData.Record;
        prescription.Date = prescription.Date == default ? DateTime.UtcNow : prescription.Date;
        await prescriptionRepository.CreateAsync(prescription);
    }

    public async Task UpdatePatientAsync(int patientId, UpdatePatientRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var patient = await patientRepository.GetByIdAsync(patientId)
            ?? throw new ArgumentException("Patient not found.");

        patient.FirstName = request.FirstName;
        patient.LastName = request.LastName;
        patient.Cnp = request.Cnp;
        patient.DateOfBirth = request.DateOfBirth;
        patient.DateOfDeath = request.DateOfDeath;
        patient.Sex = request.Sex;
        patient.PhoneNumber = request.PhoneNumber;
        patient.EmergencyContact = request.EmergencyContact;
        patient.IsArchived = request.IsArchived;
        patient.IsDonor = request.IsDonor;
        patient.Transferred = request.Transferred;

        await UpdatePatientAsync(patient, cancellationToken);
    }

    public async Task<Hospital.Data.Models.MedicalHistory?> GetMedicalHistoryAsync(int patientId)
        => (await patientRepository.GetByIdAsync(patientId))?.MedicalHistory;

    public async Task<List<Hospital.Data.Models.MedicalRecord>> GetMedicalRecordsAsync(int historyId)
        => (await patientRepository.GetAllAsync())
            .Select(patient => patient.MedicalHistory)
            .Where(history => history?.MedicalHistoryId == historyId)
            .SelectMany(history => history!.MedicalRecords)
            .ToList();

    public async Task<bool> ExistsAsync(string cnp)
        => (await patientRepository.GetAllAsync())
            .Any(patient => string.Equals(patient.Cnp, cnp, StringComparison.Ordinal));
}

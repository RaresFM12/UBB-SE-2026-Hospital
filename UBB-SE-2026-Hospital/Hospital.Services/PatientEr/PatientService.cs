using Hospital.Data.Models.DTOs;
using Hospital.Data.Repositories;
using Hospital.Shared.Models.PatientEr;
using Hospital.Shared.Services;

namespace Hospital.Services.PatientEr;

public class PatientService(
    IPatientRepository patientRepository,
    IPrescriptionRepository prescriptionRepository) : IPatientService
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

    public async Task<IReadOnlyList<Patient>> GetPatientsAsync(CancellationToken cancellationToken = default)
    {
        var dbPatients = await patientRepository.GetAllAsync();
        return dbPatients.Select(MapPatient).ToList();
    }

    public async Task<Patient> GetPatientDetailsAsync(int patientId, CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdAsync(patientId)
            ?? throw new ArgumentException("Patient not found.");
        return MapPatient(patient);
    }

    public async Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId, CancellationToken cancellationToken)
    {
        var prescription = (await prescriptionRepository.GetByRecordIdAsync(recordId)).FirstOrDefault();
        return prescription is null ? null : MapPrescription(prescription);
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

    public async Task CreatePrescriptionAsync(int recordId, Prescription prescription)
    {
        var exportData = await GetRecordExportDataAsync(recordId, CancellationToken.None);
        await prescriptionRepository.CreateAsync(new Data.Models.Prescription
        {
            MedicalRecord = exportData.Record,
            Date = prescription.DateIssued == default ? DateTime.UtcNow : prescription.DateIssued,
            DoctorNotes = prescription.Notes,
        });
    }

    private static Patient MapPatient(Data.Models.Patient patient)
    {
        return new Patient
        {
            PatientId = patient.PatientId,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Cnp = patient.Cnp,
            DateOfBirth = patient.DateOfBirth,
            DateOfDeath = patient.DateOfDeath,
            Sex = patient.Sex.ToString()[0],
            PhoneNo = patient.PhoneNumber,
            EmergencyContact = patient.EmergencyContact,
            IsArchived = patient.IsArchived,
            IsDonor = patient.IsDonor,
            Transferred = patient.Transferred,
            MedicalHistory = patient.MedicalHistory is null ? null : MapMedicalHistory(patient.MedicalHistory),
        };
    }

    private static MedicalHistory MapMedicalHistory(Data.Models.MedicalHistory medicalHistory)
    {
        return new MedicalHistory
        {
            MedicalHistoryId = medicalHistory.MedicalHistoryId,
            BloodType = medicalHistory.BloodType,
            Rh = medicalHistory.Rh,
            ChronicConditions = medicalHistory.ChronicConditions,
            MedicalRecords = medicalHistory.MedicalRecords.Select(MapMedicalRecord).ToList(),
            PatientAllergies = medicalHistory.PatientAllergies,
        };
    }

    private static MedicalRecord MapMedicalRecord(Data.Models.MedicalRecord record)
    {
        return new MedicalRecord
        {
            RecordId = record.RecordId,
            SourceType = record.SourceType,
            SourceId = record.SourceId,
            StaffMember = record.StaffMember,
            Symptoms = record.Symptoms,
            Diagnosis = record.Diagnosis,
            ConsultationDate = record.ConsultationDate,
            Prescription = record.Prescription is null ? null : MapPrescription(record.Prescription),
            BasePrice = record.BasePrice,
            FinalPrice = record.FinalPrice,
            DiscountApplied = record.DiscountApplied,
            PoliceNotified = record.PoliceNotified,
            Transplant = record.Transplant,
        };
    }

    private static Prescription MapPrescription(Data.Models.Prescription prescription)
    {
        return new Prescription
        {
            PrescriptionId = prescription.PrescriptionId,
            PatientId = prescription.MedicalRecord?.MedicalHistory?.Patient?.PatientId ?? 0,
            DoctorId = prescription.MedicalRecord?.StaffMember?.StaffId ?? 0,
            DateIssued = prescription.Date,
            Notes = prescription.DoctorNotes ?? string.Empty,
        };
    }
}

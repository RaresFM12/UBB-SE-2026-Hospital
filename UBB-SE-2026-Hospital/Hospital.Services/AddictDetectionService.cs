using System.Globalization;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;

namespace Hospital.Services;

public class AddictDetectionService(
    IPrescriptionRepository prescriptionRepository,
    IMedicalHistoryRepository medicalHistoryRepository) : IAddictDetectionService
{
    private const string ReportHeader = "==================================================\n           LAW ENFORCEMENT ALERT REPORT           \n==================================================";
    private const string ReportFooter = "--------------------------------------------------\nSUSPICIOUS ACTIVITY: SUSPECTED DRUG SHOPPING BEHAVIOR\nCRITERIA MET: MULTIPLE DOCTORS (>=3) WITHIN 30 DAYS\n--- SUPPORTING EVIDENCE (MEDICAL RECORDS) ---";
    private const string ReportPharmacistFooter = "==================================================\nACTION REQUIRED: AWAITING PHARMACIST CONFIRMATION.";
    private const int SuspiciousPrescriptionPeriodDays = 30;
    private const string NoConditionsReportedText = "None reported.";
    private const string UnknownMedicationText = "Unknown";
    private const string NoMatchingRecordsText = "No matching records pulled for this timeframe.";
    private const string ReportMedicationSeparator = ", ";

    public async Task<List<Patient>> GetAddictCandidatesAsync()
    {
        List<Prescription> flaggedPrescriptions = await prescriptionRepository.GetPotentialDrugAddictsAsync();

        List<Patient> flaggedPatients = flaggedPrescriptions
            .Where(patient => patient.MedicalRecord?.MedicalHistory?.Patient is not null)
            .Select(patient => patient.MedicalRecord.MedicalHistory.Patient)
            .DistinctBy(patient => patient.PatientId)
            .ToList();

        List<int> notifiedIds = await prescriptionRepository.GetPoliceNotifiedPatientIdsAsync(
            flaggedPatients.Select(patient => patient.PatientId));

        foreach (Patient patient in flaggedPatients)
        {
            patient.IsPoliceNotified = notifiedIds.Contains(patient.PatientId);
            patient.MedicalHistory ??= await medicalHistoryRepository.GetByPatientIdAsync(patient.PatientId);
            NormalizeMedicalHistory(patient);
        }

        return flaggedPatients;
    }

    public async Task MarkPoliceNotifiedAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Invalid patient ID.");

        await prescriptionRepository.MarkPoliceNotifiedAsync(patientId);
    }

    public async Task<string> BuildPoliceReportAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Invalid patient ID.");

        var filter = new Hospital.Data.Models.PrescriptionFilter
        {
            PatientId = patientId,
            DateFrom = DateTime.Today.AddDays(-SuspiciousPrescriptionPeriodDays),
        };

        List<Prescription> recentPrescriptions = await prescriptionRepository.GetFilteredAsync(filter);

        Patient? patient = recentPrescriptions
            .FirstOrDefault()?.MedicalRecord?.MedicalHistory?.Patient
            ?? throw new ArgumentException("Patient not found.");

        return BuildPoliceReportText(patient, recentPrescriptions);
    }

    public async Task<string> GetChronicConditionsAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Invalid Patient ID.");

        MedicalHistory? history = await medicalHistoryRepository.GetByPatientIdAsync(patientId);
        if (history is null) return NoConditionsReportedText;

        return history.ChronicConditions is { Count: > 0 }
            ? string.Join(ReportMedicationSeparator, history.ChronicConditions)
            : NoConditionsReportedText;
    }

    private static void NormalizeMedicalHistory(Patient patient)
    {
        patient.MedicalHistory ??= new MedicalHistory
        {
            ChronicConditions = [NoConditionsReportedText],
        };

        if (patient.MedicalHistory.ChronicConditions is null || patient.MedicalHistory.ChronicConditions.Count == 0)
            patient.MedicalHistory.ChronicConditions = [NoConditionsReportedText];
    }

    private static string BuildPoliceReportText(Patient patient, List<Prescription> recentPrescriptions)
    {
        var reportBuilder = new System.Text.StringBuilder();

        reportBuilder
            .AppendLine(ReportHeader)
            .AppendLine(CultureInfo.InvariantCulture, $"DATE GENERATED: {DateTime.Now:yyyy-MM-dd HH:mm}")
            .AppendLine(CultureInfo.InvariantCulture, $"SUBJECT: {patient.FirstName} {patient.LastName} (CNP: {patient.Cnp})")
            .AppendLine(CultureInfo.InvariantCulture, $"CONTACT: {patient.PhoneNumber}")
            .AppendLine(ReportFooter);

        if (recentPrescriptions.Count == 0)
        {
            reportBuilder.AppendLine(NoMatchingRecordsText);
        }
        else
        {
            int evidenceCount = 1;
            foreach (Prescription prescription in recentPrescriptions)
            {
                string meds = prescription.MedicationList?.Count > 0
                    ? string.Join(ReportMedicationSeparator, prescription.MedicationList.Select(medication => medication.MedicationName))
                    : UnknownMedicationText;

                reportBuilder
                    .AppendLine(CultureInfo.InvariantCulture, $"[{evidenceCount}] Prescription ID: {prescription.PrescriptionId} | Date: {prescription.Date:yyyy-MM-dd}")
                    .AppendLine(CultureInfo.InvariantCulture, $"    Dispensed Drugs: {meds}")
                    .AppendLine();

                evidenceCount++;
            }
        }

        reportBuilder.AppendLine(ReportPharmacistFooter);
        return reportBuilder.ToString();
    }
}

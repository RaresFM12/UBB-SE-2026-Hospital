using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services;

public class BloodCompatibilityService(
    IPatientRepository patientRepository,
    IMedicalHistoryRepository historyRepository) : IBloodCompatibilityService
{
    private const int MaxCompatibleDonors = 20;
    private const int NoCompatibilityScore = 0;
    private const int ExactBloodRhMatchScore = 50;
    private const int PartialBloodRhMatchScore = 25;
    private const int MaxAgeScore = 30;
    private const int AgeScoreStepYears = 5;
    private const int SameSexScore = 20;
    private const int DifferentSexScore = 10;

    public async Task<List<Hospital.Data.Models.Patient>> GetTopCompatibleDonorsAsync(int recipientId)
    {
        Patient? recipient = await patientRepository.GetByIdAsync(recipientId);

        if (recipient is not null)
            recipient.MedicalHistory = await historyRepository.GetByPatientIdAsync(recipientId);

        if (recipient?.MedicalHistory?.BloodType is null || recipient.MedicalHistory.Rh is null)
            return [];

        List<Patient> allPatients = await patientRepository.GetAllAsync();
        var rankedDonors = new List<(Patient Donor, int Score)>();

        foreach (Patient donor in allPatients)
        {
            if (donor.PatientId == recipientId || donor.IsDeceased || donor.IsArchived)
                continue;

            donor.MedicalHistory = await historyRepository.GetByPatientIdAsync(donor.PatientId);

            if (donor.MedicalHistory?.BloodType is null || donor.MedicalHistory.Rh is null)
                continue;

            if (!IsBloodMatch(donor.MedicalHistory.BloodType, recipient.MedicalHistory.BloodType.Value))
                continue;

            if (!IsRhMatch(donor.MedicalHistory.Rh, recipient.MedicalHistory.Rh.Value))
                continue;

            if (donor.MedicalHistory.Allergies.Any(a =>
                a.SeverityLevel.Equals("Anaphylactic", StringComparison.OrdinalIgnoreCase)))
                continue;

            rankedDonors.Add((donor, CalculateScore(donor, recipient)));
        }

        return rankedDonors
        .OrderByDescending(organ => organ.Score)
        .Select(organ => organ.Donor)
        .Take(MaxCompatibleDonors)
        .Select(patient => new Hospital.Data.Models.Patient
        {
            PatientId = patient.PatientId,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Cnp = patient.Cnp,
            DateOfBirth = patient.DateOfBirth,
            Sex = patient.Sex,
            IsArchived = patient.IsArchived,
            MedicalHistory = patient.MedicalHistory is null
                ? null
                : new Hospital.Data.Models.MedicalHistory
                {
                    BloodType = patient.MedicalHistory.BloodType,
                    Rh = patient.MedicalHistory.Rh
                }
        })
        .ToList();
    }

    public int CalculateScore(Patient donor, Patient recipient)
    {
        if (donor.MedicalHistory is null || recipient.MedicalHistory is null)
            return NoCompatibilityScore;

        int total = donor.MedicalHistory.BloodType == recipient.MedicalHistory.BloodType
            && donor.MedicalHistory.Rh == recipient.MedicalHistory.Rh
            ? ExactBloodRhMatchScore
            : PartialBloodRhMatchScore;

        int ageGap = Math.Abs(donor.DateOfBirth.Year - recipient.DateOfBirth.Year);
        total += Math.Max(NoCompatibilityScore, MaxAgeScore - ageGap / AgeScoreStepYears * AgeScoreStepYears);
        total += donor.Sex == recipient.Sex ? SameSexScore : DifferentSexScore;

        return total;
    }

    public bool IsBloodMatch(BloodType? donor, BloodType receiver)
    {
        if (donor is null) return false;
        return donor switch
        {
            BloodType.O => true,
            BloodType.A => receiver is BloodType.A or BloodType.AB,
            BloodType.B => receiver is BloodType.B or BloodType.AB,
            BloodType.AB => receiver == BloodType.AB,
            _ => false,
        };
    }

    public bool IsRhMatch(Rh? donor, Rh receiver)
    {
        if (donor is null) return false;
        return receiver != Rh.Negative || donor == Rh.Negative;
    }
}

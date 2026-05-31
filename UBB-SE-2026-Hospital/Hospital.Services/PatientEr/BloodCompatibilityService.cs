using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;

namespace Hospital.Services.PatientEr;

public class BloodCompatibilityService(
    IPatientRepository patientRepository,
    IMedicalHistoryRepository historyRepository) : IBloodCompatibilityService
{
    public async Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId)
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
            if (donor.PatientId == recipientId || donor.IsDeceased)
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
            .OrderByDescending(x => x.Score)
            .Select(x => x.Donor)
            .Take(20)
            .ToList();
    }

    public int CalculateScore(Patient donor, Patient recipient)
    {
        if (donor.MedicalHistory is null || recipient.MedicalHistory is null)
            return 0;

        int total = donor.MedicalHistory.BloodType == recipient.MedicalHistory.BloodType
            && donor.MedicalHistory.Rh == recipient.MedicalHistory.Rh ? 50 : 25;

        int ageGap = Math.Abs(donor.DateOfBirth.Year - recipient.DateOfBirth.Year);
        total += Math.Max(0, 30 - ageGap / 5 * 5);
        total += donor.Sex == recipient.Sex ? 20 : 10;

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

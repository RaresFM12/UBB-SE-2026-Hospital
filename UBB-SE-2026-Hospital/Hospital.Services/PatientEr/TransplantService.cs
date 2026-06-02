using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;

namespace Hospital.Services.PatientEr;

public class TransplantService(
    ITransplantRepository transplantRepository,
    IPatientRepository patientRepository,
    IMedicalRecordRepository recordRepository,
    IBloodCompatibilityService bloodCompatibilityService,
    IMedicalHistoryRepository historyRepository) :
    ITransplantService,
    Hospital.Shared.Services.ITransplantService
{
    private const int MaxScoreModifier = 20;
    private const int MinScoreModifier = 5;
    private const int ComparativeERVisitCount = 10;
    private const int TimeIntervalMonths = 3;

    public Task<List<Transplant>> GetAllAsync()
        => transplantRepository.GetAllAsync();

    public Task<Transplant?> GetByIdAsync(int id)
        => transplantRepository.GetByIdAsync(id);

    public Task<Transplant> CreateAsync(Transplant transplant)
        => transplantRepository.CreateAsync(transplant);

    public Task<Transplant> UpdateAsync(Transplant transplant)
        => transplantRepository.UpdateAsync(transplant);

    public Task DeleteAsync(int id)
        => transplantRepository.DeleteAsync(id);

    public Task<List<Transplant>> GetByPatientIdAsync(int patientId)
        => transplantRepository.GetByPatientIdAsync(patientId);

    public async Task<List<Transplant>> GetByReceiverIdAsync(int receiverId)
        => (await transplantRepository.GetAllAsync())
            .Where(t => t.Receiver?.PatientId == receiverId)
            .ToList();

    public async Task<List<Transplant>> GetByDonorIdAsync(int donorId)
        => (await transplantRepository.GetAllAsync())
            .Where(t => t.Donor?.PatientId == donorId)
            .ToList();

    public Task<List<TransplantMatch>> GetTopMatchesForDonorAsync(int donorId, string organType)
        => GetTopMatchesAsDisplayModelsAsync(donorId, organType);

    public async Task<List<TransplantMatch>> GetMatchesAsync()
    {
        List<Transplant> transplants = await transplantRepository.GetAllAsync();
        var matches = new List<TransplantMatch>();
        foreach (Transplant transplant in transplants)
        {
            matches.AddRange(await transplantRepository.GetMatchesForTransplantAsync(transplant.TransplantId));
        }

        return matches;
    }

    public async Task CreateWaitlistRequestAsync(int receiverId, string organType)
    {
        Patient? receiver = await patientRepository.GetByIdAsync(receiverId)
            ?? throw new ArgumentException("Receiver not found.");

        var transplant = new Transplant
        {
            Receiver = receiver,
            Donor = null,
            OrganType = organType.Trim(),
            RequestDate = DateTime.UtcNow,
            Status = TransplantStatus.Pending,
            CompatibilityScore = 0,
        };

        await transplantRepository.CreateAsync(transplant);
    }

    public async Task AssignDonorAsync(int transplantId, int donorId, float finalScore)
    {
        Transplant transplant = await transplantRepository.GetByIdAsync(transplantId)
            ?? throw new ArgumentException($"Transplant {transplantId} not found.");

        Patient donor = await patientRepository.GetByIdAsync(donorId)
            ?? throw new ArgumentException($"Donor {donorId} not found.");

        transplant.Donor = donor;
        transplant.CompatibilityScore = finalScore;
        transplant.Status = TransplantStatus.Completed;

        await transplantRepository.UpdateAsync(transplant);
    }

    public async Task<List<TransplantMatch>> GetTopMatchesAsDisplayModelsAsync(int donorId, string organType)
    {
        Patient? donor = await patientRepository.GetByIdAsync(donorId)
            ?? throw new InvalidOperationException("Donor not found.");

        if (!donor.IsDeceased || !donor.IsDonor)
            throw new InvalidOperationException("Donor must be deceased and registered.");

        donor.MedicalHistory = await historyRepository.GetByPatientIdAsync(donor.PatientId);

        string normalizedOrgan = organType.Trim();
        List<Transplant> allTransplants = await transplantRepository.GetAllAsync();
        List<Transplant> waitlist = allTransplants
            .Where(t => t.Status == TransplantStatus.Pending
                && string.Equals(t.OrganType, normalizedOrgan, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var matches = new List<TransplantMatch>();

        foreach (Transplant request in waitlist)
        {
            Patient? receiver = request.Receiver
                ?? await patientRepository.GetByIdAsync(request.Receiver.PatientId);

            if (receiver is null) continue;

            receiver.MedicalHistory = await historyRepository.GetByPatientIdAsync(receiver.PatientId);

            if (receiver.MedicalHistory?.BloodType is null || receiver.MedicalHistory.Rh is null)
                continue;

            if (!bloodCompatibilityService.IsBloodMatch(donor.MedicalHistory?.BloodType, receiver.MedicalHistory.BloodType.Value))
                continue;

            if (!bloodCompatibilityService.IsRhMatch(donor.MedicalHistory?.Rh, receiver.MedicalHistory.Rh.Value))
                continue;

            float score = await CalculatePostMortemScoreAsync(donor, receiver);

            matches.Add(new TransplantMatch
            {
                Transplant = request,
                Receiver = receiver,
                ReceiverName = receiver.FullName,
                BloodType = receiver.MedicalHistory.BloodType?.ToString() ?? "Unknown",
                CompatibilityScore = score,
                RequestDate = request.RequestDate,
                WaitingDays = (DateTime.UtcNow - request.RequestDate).Days,
            });
        }

        return matches
            .OrderByDescending(m => m.CompatibilityScore)
            .ThenBy(m => m.RequestDate)
            .Take(5)
            .ToList();
    }

    public async Task<bool> IsUrgentAsync(int patientId)
    {
        MedicalHistory? history = await historyRepository.GetByPatientIdAsync(patientId);
        if (history is null) return false;

        DateTime threeMonthsAgo = DateTime.UtcNow.AddMonths(-TimeIntervalMonths);
        List<MedicalRecord> records = await recordRepository.GetByMedicalHistoryIdAsync(history.MedicalHistoryId);

        int erVisitCount = records.Count(r =>
            r.SourceType == SourceType.ER && r.ConsultationDate >= threeMonthsAgo);

        return erVisitCount >= ComparativeERVisitCount;
    }

    public async Task<string?> GetChronicWarningAsync(int patientId)
    {
        MedicalHistory? history = await historyRepository.GetByPatientIdAsync(patientId);
        if (history?.ChronicConditions is { Count: > 0 })
            return "Patient has underlying conditions that may affect transplant success.";

        return null;
    }

    private async Task<float> CalculatePostMortemScoreAsync(Patient donor, Patient receiver)
    {
        float score = bloodCompatibilityService.CalculateScore(donor, receiver);

        MedicalHistory? receiverHistory = await historyRepository.GetByPatientIdAsync(receiver.PatientId);
        if (receiverHistory is null) return score;

        DateTime threeMonthsAgo = DateTime.UtcNow.AddMonths(-TimeIntervalMonths);
        List<MedicalRecord> records = await recordRepository.GetByMedicalHistoryIdAsync(receiverHistory.MedicalHistoryId);

        int erVisits = records.Count(r =>
            r.SourceType == SourceType.ER && r.ConsultationDate >= threeMonthsAgo);

        score += erVisits >= ComparativeERVisitCount ? MaxScoreModifier : MinScoreModifier;
        return score;
    }
}

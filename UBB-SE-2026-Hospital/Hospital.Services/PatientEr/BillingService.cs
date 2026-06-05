using Hospital.Data.Models;
using Hospital.Data.Repositories;

namespace Hospital.Services.PatientEr;

public class BillingService(
    IMedicalHistoryRepository historyRepository,
    IMedicalRecordRepository recordRepository,
    IPrescriptionRepository prescriptionRepository,
    ITransplantRepository transplantRepository) : Hospital.Shared.Services.IBillingService
{
    private const int PercentageDivisor = 100;
    private const decimal EmergencyRoomBasePrice = 500;
    private const decimal AppointmentBasePrice = 200;
    private const decimal PrescriptionItemPrice = 50;
    private const decimal ChronicConditionPrice = 100;
    private const decimal MildOrModerateAllergyPrice = 20;
    private const decimal SevereAllergyPrice = 100;
    private const decimal TransplantAdditionalPrice = 2000;

    private const string MildSeverity = "mild";
    private const string ModerateSeverity = "moderate";
    private const string SevereSeverity = "severe";
    private const string AnaphylacticSeverity = "anaphylactic";

    public async Task<decimal> ComputeBasePriceAsync(int patientId, int recordId)
    {
        MedicalRecord? record = await recordRepository.GetByIdAsync(recordId);
        List<PrescriptionItem> prescriptionItems = [];

        if (record?.Prescription is not null)
            prescriptionItems = await prescriptionRepository.GetItemsAsync(record.Prescription.PrescriptionId);

        MedicalHistory? history = await historyRepository.GetByPatientIdAsync(patientId);
        List<Transplant> associatedTransplants = await transplantRepository.GetByPatientIdAsync(patientId);

        return CalculateBasePrice(record, history, prescriptionItems, associatedTransplants);
    }

    public async Task<decimal> ApplyDiscountAsync(decimal basePrice, int discount)
        => await Task.FromResult(basePrice - basePrice * discount / PercentageDivisor);

    public async Task<decimal> PersistDiscountAsync(int recordId, decimal basePrice, int discount)
    {
        decimal finalPrice = await ApplyDiscountAsync(basePrice, discount);

        MedicalRecord record = await recordRepository.GetByIdAsync(recordId)
            ?? throw new KeyNotFoundException($"Medical record with ID {recordId} not found.");

        record.BasePrice = basePrice;
        record.DiscountApplied = discount;
        record.FinalPrice = finalPrice;

        await recordRepository.UpdateAsync(record);
        return finalPrice;
    }

    private static decimal CalculateBasePrice(
        MedicalRecord? record,
        MedicalHistory? history,
        List<PrescriptionItem> prescriptionItems,
        List<Transplant> associatedTransplants)
    {
        if (record is null || history is null)
            return 0;

        decimal score = record.SourceType == SourceType.ER
            ? EmergencyRoomBasePrice
            : AppointmentBasePrice;

        score += PrescriptionItemPrice * prescriptionItems.Count;
        score += ChronicConditionPrice * history.ChronicConditions.Count;

        foreach ((Allergy allergy, string severityLevel) in history.Allergies)
        {
            if (string.Equals(severityLevel, MildSeverity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(severityLevel, ModerateSeverity, StringComparison.OrdinalIgnoreCase))
                score += MildOrModerateAllergyPrice;
            else if (string.Equals(severityLevel, SevereSeverity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(severityLevel, AnaphylacticSeverity, StringComparison.OrdinalIgnoreCase))
                score += SevereAllergyPrice;
        }

        if (associatedTransplants.Count > 0)
            score += TransplantAdditionalPrice;

        return score;
    }
}

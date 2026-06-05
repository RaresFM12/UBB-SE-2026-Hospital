using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class SalaryComputationService(
    IStaffRepository staffRepository,
    IShiftRepository shiftRepository,
    IPharmacyHandoverRepository pharmacyHandoverRepository,
    IHangoutRepository hangoutRepository,
    IHangoutParticipantRepository hangoutParticipantRepository) : ISalaryComputationService
{
    private const double DoctorBaseHourlyRate = 85.0;
    private const double PharmacistBaseHourlyRate = 45.0;

    private const double SaturdayOvertimeMultiplier = 1.15;
    private const double SundayOvertimeMultiplier = 1.25;

    private const int NightShiftStartHour = 20;
    private const int NightShiftEndHour = 6;
    private const double NightShiftOvertimeMultiplier = 1.20;

    private const double SurgeonSpecializationBonusPercentage = 0.20;
    private const double CardiologistSpecializationBonusPercentage = 0.15;
    private const double EmergencySpecializationBonusPercentage = 0.10;

    private const double YearsOfExperienceBonusPercentagePerYear = 0.02;
    private const double HangoutParticipationBonusMultiplier = 1.05;

    private const int MedicinesSoldBonusInterval = 10;
    private const double MedicinesSoldBonusPerInterval = 0.01;
    private const double MaxMedicineSalesBonusPercentage = 0.30;

    public async Task<IReadOnlyList<Staff>> GetAllStaffAsync(CancellationToken cancellationToken = default)
        => await staffRepository.GetAllAsync();

    public async Task<IReadOnlyList<Shift>> GetAllShiftsAsync(CancellationToken cancellationToken = default)
        => await shiftRepository.GetAllAsync();

    public async Task<double> ComputeSalaryDoctorAsync(Doctor doctor, IReadOnlyList<Shift> monthlyShifts, int month, int year, CancellationToken cancellationToken = default)
    {
        double baseSalary = ComputeBaseSalaryFromShifts(monthlyShifts, DoctorBaseHourlyRate);
        double specializationBonus = ResolveSpecializationBonusPercentage(doctor.Specialization);

        double finalSalary = baseSalary;
        finalSalary += baseSalary * specializationBonus;
        finalSalary += baseSalary * (doctor.YearsOfExperience * YearsOfExperienceBonusPercentagePerYear);

        if (await DidStaffParticipateInHangoutForMonthAsync(doctor.StaffId, month, year))
        {
            finalSalary *= HangoutParticipationBonusMultiplier;
        }

        return finalSalary;
    }

    public async Task<double> ComputeSalaryPharmacistAsync(Pharmacyst pharmacist, IReadOnlyList<Shift> monthlyShifts, int month, int year, CancellationToken cancellationToken = default)
    {
        double baseSalary = ComputeBaseSalaryFromShifts(monthlyShifts, PharmacistBaseHourlyRate);

        int medicinesSold = await CountMedicinesSoldForPharmacistAsync(pharmacist.StaffId, month, year);
        double salesBonus = Math.Min((medicinesSold / MedicinesSoldBonusInterval) * MedicinesSoldBonusPerInterval, MaxMedicineSalesBonusPercentage);

        double finalSalary = baseSalary;
        finalSalary += baseSalary * salesBonus;
        finalSalary += baseSalary * (pharmacist.YearsOfExperience * YearsOfExperienceBonusPercentagePerYear);

        if (await DidStaffParticipateInHangoutForMonthAsync(pharmacist.StaffId, month, year))
        {
            finalSalary *= HangoutParticipationBonusMultiplier;
        }

        return finalSalary;
    }

    private static double ComputeBaseSalaryFromShifts(IReadOnlyList<Shift> shifts, double baseHourlyRate)
    {
        double total = 0;
        foreach (var shift in shifts)
        {
            double shiftHours = (shift.EndTime - shift.StartTime).TotalHours;
            double shiftSalary = shiftHours * baseHourlyRate;

            if (shift.StartTime.DayOfWeek == DayOfWeek.Saturday)
            {
                shiftSalary *= SaturdayOvertimeMultiplier;
            }
            else if (shift.StartTime.DayOfWeek == DayOfWeek.Sunday)
            {
                shiftSalary *= SundayOvertimeMultiplier;
            }

            bool isNightShift = shift.StartTime.Hour >= NightShiftStartHour
                || shift.StartTime.Hour <= NightShiftEndHour
                || shift.EndTime.Hour <= NightShiftEndHour;

            if (isNightShift)
            {
                shiftSalary *= NightShiftOvertimeMultiplier;
            }

            total += shiftSalary;
        }

        return total;
    }

    private static double ResolveSpecializationBonusPercentage(string? specialization)
    {
        string normalized = (specialization ?? string.Empty).ToLowerInvariant();

        if (normalized.Contains("surgeon") || normalized.Contains("surgery"))
        {
            return SurgeonSpecializationBonusPercentage;
        }

        if (normalized.Contains("cardiologist"))
        {
            return CardiologistSpecializationBonusPercentage;
        }

        if (normalized.Contains("er") || normalized.Contains("emergency"))
        {
            return EmergencySpecializationBonusPercentage;
        }

        return 0;
    }

    private async Task<int> CountMedicinesSoldForPharmacistAsync(int pharmacistStaffId, int month, int year)
    {
        var handovers = await pharmacyHandoverRepository.GetAllAsync();
        bool MatchesPharmacistAndMonth(PharmacyHandover handover) =>
            handover.Pharmacist.StaffId == pharmacistStaffId
            && handover.HandoverDate.Month == month
            && handover.HandoverDate.Year == year;
        return handovers.Count(MatchesPharmacistAndMonth);
    }

    private async Task<bool> DidStaffParticipateInHangoutForMonthAsync(int staffId, int month, int year)
    {
        var participations = await hangoutParticipantRepository.GetByStaffIdAsync(staffId);
        var hangoutIds = participations.Select(participation => participation.Hangout.HangoutID).ToHashSet();

        if (hangoutIds.Count == 0)
        {
            return false;
        }

        var hangouts = await hangoutRepository.GetAllAsync();
        bool IsHangoutInTargetMonth(Hangout hangout) =>
            hangoutIds.Contains(hangout.HangoutID)
            && hangout.Date.Month == month
            && hangout.Date.Year == year;

        return hangouts.Any(IsHangoutInTargetMonth);
    }
}

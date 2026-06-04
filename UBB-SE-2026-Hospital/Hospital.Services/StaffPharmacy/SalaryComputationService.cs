using Hospital.Data.Repositories;
using Hospital.Shared.Services;
using Hospital.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hospital.Services.StaffPharmacy
{
    public class SalaryComputationService : ISalaryComputationService
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

        private readonly IPharmacyHandoverRepository pharmacyHandoverRepository;
        private readonly IHangoutRepository hangoutRepository;
        private readonly IHangoutParticipantRepository hangoutParticipantRepository;
        private readonly IStaffRepository staffRepository;
        private readonly IShiftRepository shiftRepository;

        public SalaryComputationService(
            IPharmacyHandoverRepository pharmacyHandoverRepository,
            IHangoutRepository hangoutRepository,
            IHangoutParticipantRepository hangoutParticipantRepository,
            IStaffRepository staffRepository,
            IShiftRepository shiftRepository)
        {
            this.pharmacyHandoverRepository = pharmacyHandoverRepository;
            this.hangoutRepository = hangoutRepository;
            this.hangoutParticipantRepository = hangoutParticipantRepository;
            this.staffRepository = staffRepository;
            this.shiftRepository = shiftRepository;
        }

        public async Task<double> ComputeSalaryDoctorAsync(Doctor doctor, IReadOnlyList<Shift> monthlyShifts, int month, int year, CancellationToken cancellationToken = default)
        {
            double baseSalaryFromShifts = this.ComputeBaseSalaryFromShifts(monthlyShifts, DoctorBaseHourlyRate);
            double specializationBonusPercentage = ResolveSpecializationBonusPercentage(doctor.Specialization);
            double finalSalary = baseSalaryFromShifts;
            finalSalary += baseSalaryFromShifts * specializationBonusPercentage;
            finalSalary += baseSalaryFromShifts * (doctor.YearsOfExperience * YearsOfExperienceBonusPercentagePerYear);

            if (await this.DidStaffParticipateInHangoutForMonthAsync(doctor.StaffID, month, year))
            {
                finalSalary *= HangoutParticipationBonusMultiplier;
            }

            return finalSalary;
        }

        public async Task<double> ComputeSalaryPharmacistAsync(Pharmacyst pharmacist, IReadOnlyList<Shift> monthlyShifts, int month, int year, CancellationToken cancellationToken = default)
        {
            double baseSalaryFromShifts = this.ComputeBaseSalaryFromShifts(monthlyShifts, PharmacistBaseHourlyRate);
            int medicinesSold = this.CountMedicinesSoldForPharmacist(pharmacist.StaffID, month, year);
            double medicineSalesBonusPercentage = Math.Min((medicinesSold / MedicinesSoldBonusInterval) * MedicinesSoldBonusPerInterval, MaxMedicineSalesBonusPercentage);

            double finalSalary = baseSalaryFromShifts;
            finalSalary += baseSalaryFromShifts * medicineSalesBonusPercentage;
            finalSalary += baseSalaryFromShifts * (pharmacist.YearsOfExperience * YearsOfExperienceBonusPercentagePerYear);

            if (await this.DidStaffParticipateInHangoutForMonthAsync(pharmacist.StaffID, month, year))
            {
                finalSalary *= HangoutParticipationBonusMultiplier;
            }

            return finalSalary;
        }

        public async Task<IReadOnlyList<Staff>> GetAllStaffAsync(CancellationToken cancellationToken = default)
        {
            var staff = await this.staffRepository.GetAllAsync();
            return staff.Cast<Staff>().ToList();
        }

        public async Task<IReadOnlyList<Shift>> GetAllShiftsAsync(CancellationToken cancellationToken = default)
        {
            return await this.shiftRepository.GetAllAsync();
        }

        private double ComputeBaseSalaryFromShifts(IReadOnlyList<Shift> monthlyShifts, double baseHourlyRate)
        {
            double total = 0;
            foreach (var shift in monthlyShifts)
            {
                double shiftHours = (shift.EndTime - shift.StartTime).TotalHours;
                double shiftSalary = shiftHours * baseHourlyRate;

                if (shift.StartTime.DayOfWeek == DayOfWeek.Saturday) shiftSalary *= SaturdayOvertimeMultiplier;
                else if (shift.StartTime.DayOfWeek == DayOfWeek.Sunday) shiftSalary *= SundayOvertimeMultiplier;

                if (shift.StartTime.Hour >= NightShiftStartHour || shift.StartTime.Hour <= NightShiftEndHour || shift.EndTime.Hour <= NightShiftEndHour)
                {
                    shiftSalary *= NightShiftOvertimeMultiplier;
                }
                total += shiftSalary;
            }
            return total;
        }

        private static double ResolveSpecializationBonusPercentage(string? specialization)
        {
            string norm = (specialization ?? string.Empty).ToLowerInvariant();
            if (norm.Contains("surgeon") || norm.Contains("surgery")) return SurgeonSpecializationBonusPercentage;
            if (norm.Contains("cardiologist")) return CardiologistSpecializationBonusPercentage;
            if (norm.Contains("er") || norm.Contains("emergency")) return EmergencySpecializationBonusPercentage;
            return 0;
        }

        private int CountMedicinesSoldForPharmacist(int pharmacistStaffId, int month, int year)
        {
            var allHandovers = this.pharmacyHandoverRepository.GetAllAsync().GetAwaiter().GetResult();

            return allHandovers.Count(h => h.Pharmacist.StaffID == pharmacistStaffId
                                       && h.HandoverDate.Month == month
                                       && h.HandoverDate.Year == year);
        }

        private async Task<bool> DidStaffParticipateInHangoutForMonthAsync(int staffId, int month, int year)
        {
            var participants = await this.hangoutParticipantRepository.GetByStaffIdAsync(staffId);
            var hangoutIds = participants.Select(p => p.Hangout.HangoutID).ToHashSet();

            if (!hangoutIds.Any()) return false;
            var hangouts = await this.hangoutRepository.GetAllAsync();

            return hangouts.Any(h => hangoutIds.Contains(h.HangoutID)
                                  && h.Date.Month == month
                                  && h.Date.Year == year);
        }
    }
}
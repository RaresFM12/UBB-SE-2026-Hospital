using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hospital.Services.StaffPharmacy
{
    public class PharmacyVacationService : IPharmacyVacationService
    {
        private const int OneDay = 1;
        private const string VacationShiftLocationLabel = "Vacation";

        private readonly IStaffRepository staffRepository;
        private readonly IShiftRepository shiftRepository;

        public PharmacyVacationService(IStaffRepository staffRepository, IShiftRepository shiftRepository)
        {
            this.staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
            this.shiftRepository = shiftRepository ?? throw new ArgumentNullException(nameof(shiftRepository));
        }

        public async Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default)
        {
            var pharmacists = await this.staffRepository.GetAllPharmacistsAsync();
            return pharmacists
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToList();
        }

        public IReadOnlyList<Pharmacyst> GetPharmacists()
        {
            return GetPharmacistsAsync().GetAwaiter().GetResult();
        }

        public async Task RegisterVacationAsync(int pharmacistStaffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate.Date;
            var endExclusive = endDate.Date.AddDays(OneDay);

            if (endDate.Date < start)
            {
                throw new ArgumentException("End date must be on or after start date.");
            }

            var pharmacists = await this.staffRepository.GetAllPharmacistsAsync();
            var pharmacist = pharmacists.FirstOrDefault(p => p.StaffID == pharmacistStaffId)
                ?? throw new ArgumentException("Pharmacist not found.");

            var pharmacistShifts = await this.shiftRepository.GetByStaffIdAsync(pharmacistStaffId);
            var overlappingShift = pharmacistShifts.FirstOrDefault(shift => start < shift.EndTime && endExclusive > shift.StartTime);

            if (overlappingShift is not null)
            {
                bool isExistingVacation = overlappingShift.Status == ShiftStatus.Vacation;
                throw new InvalidOperationException(isExistingVacation
                    ? "Cannot add vacation: this period overlaps an existing vacation entry."
                    : "Cannot add vacation: this period overlaps an existing shift.");
            }

            var vacationShift = new Shift(
                0,
                pharmacist,
                VacationShiftLocationLabel,
                start,
                endExclusive,
                ShiftStatus.Vacation);

            await this.shiftRepository.CreateAsync(vacationShift);
        }

        public void RegisterVacation(int pharmacistStaffId, DateTime startDate, DateTime endDate)
        {
            RegisterVacationAsync(pharmacistStaffId, startDate, endDate).GetAwaiter().GetResult();
        }
    }
}
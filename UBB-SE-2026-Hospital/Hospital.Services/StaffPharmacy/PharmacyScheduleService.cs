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
    public class PharmacyScheduleService : IPharmacyScheduleService
    {
        private readonly IShiftRepository shiftRepository;
        private readonly IStaffRepository staffRepository;

        public PharmacyScheduleService(IShiftRepository shiftRepository, IStaffRepository staffRepository)
        {
            this.shiftRepository = shiftRepository;
            this.staffRepository = staffRepository;
        }

        public async Task<IReadOnlyList<Shift>> GetShiftsAsync(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd, CancellationToken cancellationToken = default)
        {
            var allShifts = await this.shiftRepository.GetAllAsync();

            return allShifts
                .Where(shift => shift.AppointedStaff.StaffID == pharmacistStaffId
                             && shift.StartTime < rangeEnd
                             && shift.EndTime > rangeStart)
                .OrderBy(shift => shift.StartTime)
                .ToList();
        }

        public IReadOnlyList<Shift> GetShifts(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd)
        {
            return GetShiftsAsync(pharmacistStaffId, rangeStart, rangeEnd).GetAwaiter().GetResult();
        }

        public async Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default)
        {
            var pharmacists = await this.staffRepository.GetAllPharmacistsAsync();
            return pharmacists;
        }

        public IReadOnlyList<Pharmacyst> GetPharmacists()
        {
            return GetPharmacistsAsync().GetAwaiter().GetResult();
        }
    }
}
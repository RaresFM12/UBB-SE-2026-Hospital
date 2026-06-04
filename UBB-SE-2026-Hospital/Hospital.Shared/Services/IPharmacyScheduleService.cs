namespace Hospital.Shared.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Shared.Models;

public interface IPharmacyScheduleService
{
    Task<IReadOnlyList<Shift>> GetShiftsAsync(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd);

    List<Pharmacyst> GetPharmacists();
}

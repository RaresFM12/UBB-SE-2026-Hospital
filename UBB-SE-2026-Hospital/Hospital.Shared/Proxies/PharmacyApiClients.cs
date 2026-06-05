using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies
{
    public class PharmacyScheduleApiClient : ApiClientBase, IPharmacyScheduleService
    {
        public PharmacyScheduleApiClient(HttpClient httpClient) : base(httpClient) { }

        public async Task<IReadOnlyList<Shift>> GetShiftsAsync(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd, CancellationToken cancellationToken = default)
        {
            var allShifts = await GetAsync<List<Shift>>("api/shifts") ?? new List<Shift>();

            return allShifts
                .Where(shift => shift.AppointedStaff != null
                             && shift.AppointedStaff.StaffID == pharmacistStaffId
                             && shift.StartTime < rangeEnd
                             && shift.EndTime > rangeStart)
                .OrderBy(shift => shift.StartTime)
                .ToList();
        }

        public IReadOnlyList<Shift> GetShifts(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd)
            => GetShiftsAsync(pharmacistStaffId, rangeStart, rangeEnd).GetAwaiter().GetResult();

        public async Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<List<Pharmacyst>>("api/staff/pharmacists") ?? new List<Pharmacyst>();
        }

        public IReadOnlyList<Pharmacyst> GetPharmacists()
            => GetPharmacistsAsync().GetAwaiter().GetResult();
    }

    public class PharmacyVacationApiClient : ApiClientBase, IPharmacyVacationService
    {
        public PharmacyVacationApiClient(HttpClient httpClient) : base(httpClient) { }

        public async Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<List<Pharmacyst>>("api/staff/pharmacists") ?? new List<Pharmacyst>();
        }

        public IReadOnlyList<Pharmacyst> GetPharmacists()
            => GetPharmacistsAsync().GetAwaiter().GetResult();

        public async Task RegisterVacationAsync(int pharmacistStaffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            await PostAsync<object, object>("api/pharmacyvacation/register", new { pharmacistStaffId, startDate, endDate });
        }

        public void RegisterVacation(int pharmacistStaffId, DateTime startDate, DateTime endDate)
            => RegisterVacationAsync(pharmacistStaffId, startDate, endDate).GetAwaiter().GetResult();
    }
}
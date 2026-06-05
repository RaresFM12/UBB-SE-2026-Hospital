using Hospital.Data.Models;

namespace Hospital.Shared.Proxies
{
    public interface IFatigueAuditApiClient
    {
        bool ReassignShift(int shiftId, int newStaffId);
        AutoAuditResult RunAutoAudit(DateTime weekStart);
    }
}
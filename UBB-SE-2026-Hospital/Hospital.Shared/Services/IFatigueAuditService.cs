using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IFatigueAuditService
{
    AutoAuditResult RunAutoAudit(DateTime weekStart);

    bool ReassignShift(int shiftId, int newStaffId);
}

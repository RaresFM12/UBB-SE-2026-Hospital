namespace Hospital.Shared.Services
{
    using System;
    using Hospital.Shared.Models;

    public interface IFatigueAuditService
    {
        AutoAuditResult RunAutoAudit(DateTime weekStart);

        bool ReassignShift(int shiftId, int newStaffId);
    }
}

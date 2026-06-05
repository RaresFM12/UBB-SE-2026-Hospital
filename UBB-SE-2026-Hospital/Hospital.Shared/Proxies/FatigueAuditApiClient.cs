using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class FatigueAuditApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IFatigueAuditService
{
    public AutoAuditResult RunAutoAudit(DateTime weekStart)
        => Task.Run(async () => await GetAsync<AutoAuditResult>($"api/fatigueaudit/run?weekStart={weekStart:yyyy-MM-dd}")
           ?? new AutoAuditResult { HasConflicts = false, Summary = string.Empty, Violations = [], Suggestions = [] }).GetAwaiter().GetResult();

    public bool ReassignShift(int shiftId, int newStaffId)
    {
        try
        {
            Task.Run(async () => await PutAsync<object>($"api/shifts/{shiftId}/staff", new { staffId = newStaffId })).GetAwaiter().GetResult();
            return true;
        }
        catch { return false; }
    }
}



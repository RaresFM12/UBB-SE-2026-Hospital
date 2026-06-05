using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;

namespace Hospital.Web.Services;

public interface IErWorkflowApiClient
{
    Task<List<ERVisit>> GetVisitsAsync(CancellationToken cancellationToken = default);
    Task<List<ERVisit>> GetVisitsByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<ERVisit?> GetVisitAsync(int id, CancellationToken cancellationToken = default);
    Task<ERVisit> CreateVisitAsync(ERVisit visit, CancellationToken cancellationToken = default);
    Task UpdateVisitAsync(int id, ERVisit visit, CancellationToken cancellationToken = default);
    Task UpdateVisitStatusAsync(int id, string status, CancellationToken cancellationToken = default);
    Task<bool> AutoAssignHighestPriorityRoomAsync(CancellationToken cancellationToken = default);
    Task AssignRoomAsync(int visitId, int roomId, CancellationToken cancellationToken = default);
    Task TransferVisitAsync(int visitId, CancellationToken cancellationToken = default);
    Task RetryTransferAsync(int visitId, CancellationToken cancellationToken = default);
    Task CloseVisitAsync(int visitId, CancellationToken cancellationToken = default);

    Task<List<ERRoom>> GetRoomsAsync(CancellationToken cancellationToken = default);
    Task<List<ERRoom>> GetRoomsByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<ERRoomVisitDetails?> GetRoomVisitDetailsAsync(int roomId, CancellationToken cancellationToken = default);
    Task MarkRoomAsCleaningAsync(int roomId, CancellationToken cancellationToken = default);
    Task MarkRoomAsAvailableAsync(int roomId, CancellationToken cancellationToken = default);

    Task<List<Triage>> GetTriagesAsync(CancellationToken cancellationToken = default);
    Task<Triage?> GetTriageByVisitIdAsync(int visitId, CancellationToken cancellationToken = default);
    Task<Triage> CreateTriageAsync(Triage triage, CancellationToken cancellationToken = default);
    Task<PerformTriageResponse> PerformTriageAsync(PerformTriageRequest request, CancellationToken cancellationToken = default);

    Task<List<TriageParameters>> GetTriageParametersAsync(CancellationToken cancellationToken = default);
    Task<TriageParameters?> GetTriageParametersByTriageIdAsync(int triageId, CancellationToken cancellationToken = default);
    Task<TriageParameters> CreateTriageParametersAsync(TriageParameters parameters, CancellationToken cancellationToken = default);

    Task<List<ERVisit>> GetEligibleExaminationVisitsAsync(CancellationToken cancellationToken = default);
    Task<List<Examination>> GetExaminationsByVisitIdAsync(int visitId, CancellationToken cancellationToken = default);
    Task<List<Examination>> GetPatientExaminationHistoryAsync(string patientId, CancellationToken cancellationToken = default);
    Task<ERExaminationSummary?> GetExaminationSummaryAsync(int visitId, CancellationToken cancellationToken = default);
    Task<Examination> CreateExaminationAsync(Examination examination, CancellationToken cancellationToken = default);
    Task UpdateExaminationAsync(int examId, Examination examination, CancellationToken cancellationToken = default);

    Task<List<ERTransferEligibleVisit>> GetEligibleTransferVisitsAsync(CancellationToken cancellationToken = default);
    Task<List<TransferLog>> GetTransferLogsByVisitIdAsync(int visitId, CancellationToken cancellationToken = default);
}

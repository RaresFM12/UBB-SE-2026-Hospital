using Hospital.Data.Models;
using Hospital.Data.Models;

namespace Hospital.Web.Services;

public class ErWorkflowApiClient : HospitalApiClientBase, IErWorkflowApiClient
{
    private const string VisitsBaseUri = "api/ervisits";
    private const string RoomsBaseUri = "api/errooms";
    private const string TriagesBaseUri = "api/triage";
    private const string TriageParametersBaseUri = "api/triageparameters";
    private const string ExaminationsBaseUri = "api/examinations";
    private const string TransferLogsBaseUri = "api/transfer-logs";

    public ErWorkflowApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        : base(httpClient, httpContextAccessor)
    {
    }

    public async Task<List<ERVisit>> GetVisitsAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<ERVisit>>(VisitsBaseUri, cancellationToken) ?? new List<ERVisit>();

    public async Task<List<ERVisit>> GetVisitsByStatusAsync(string status, CancellationToken cancellationToken = default) =>
        (await GetVisitsAsync(cancellationToken))
            .Where(visit => string.Equals(visit.Status, status, StringComparison.OrdinalIgnoreCase))
            .ToList();

    public Task<ERVisit?> GetVisitAsync(int id, CancellationToken cancellationToken = default) =>
        GetAsync<ERVisit>($"{VisitsBaseUri}/{id}", cancellationToken);

    public async Task<ERVisit> CreateVisitAsync(ERVisit visit, CancellationToken cancellationToken = default) =>
        await PostAsync<ERVisit, ERVisit>(VisitsBaseUri, visit, cancellationToken)
        ?? throw new InvalidOperationException("Failed to create ER visit: no response from server.");

    public Task UpdateVisitAsync(int id, ERVisit visit, CancellationToken cancellationToken = default) =>
        PutAsync($"{VisitsBaseUri}/{id}", visit, cancellationToken);

    public async Task UpdateVisitStatusAsync(int id, string status, CancellationToken cancellationToken = default)
    {
        ERVisit visit = await GetVisitAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"ER visit {id} was not found.");

        visit.Status = status;
        await UpdateVisitAsync(id, visit, cancellationToken);
    }

    public async Task<bool> AutoAssignHighestPriorityRoomAsync(CancellationToken cancellationToken = default) =>
        await PostAsync<object, bool>($"{VisitsBaseUri}/auto-assign-room", new { }, cancellationToken);

    public Task AssignRoomAsync(int visitId, int roomId, CancellationToken cancellationToken = default) =>
        PostAsync<object>($"{VisitsBaseUri}/{visitId}/assign-room/{roomId}", new { }, cancellationToken);

    public Task TransferVisitAsync(int visitId, CancellationToken cancellationToken = default) =>
        PostAsync<object>($"{VisitsBaseUri}/{visitId}/transfer", new { }, cancellationToken);

    public Task RetryTransferAsync(int visitId, CancellationToken cancellationToken = default) =>
        PostAsync<object>($"{VisitsBaseUri}/{visitId}/retry-transfer", new { }, cancellationToken);

    public Task CloseVisitAsync(int visitId, CancellationToken cancellationToken = default) =>
        PostAsync<object>($"{VisitsBaseUri}/{visitId}/close", new { }, cancellationToken);

    public async Task<List<ERRoom>> GetRoomsAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<ERRoom>>(RoomsBaseUri, cancellationToken) ?? new List<ERRoom>();

    public async Task<List<ERRoom>> GetRoomsByStatusAsync(string status, CancellationToken cancellationToken = default) =>
        await GetAsync<List<ERRoom>>($"{RoomsBaseUri}/status/{status}", cancellationToken) ?? new List<ERRoom>();

    public async Task<ERRoomVisitDetails?> GetRoomVisitDetailsAsync(int roomId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<ERRoomVisitDetails>($"{RoomsBaseUri}/{roomId}/visit-details", cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
        catch (InvalidOperationException ex) when (
            ex.Message.Contains("404", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
    }

    public Task MarkRoomAsCleaningAsync(int roomId, CancellationToken cancellationToken = default) =>
        PostAsync<object>($"{RoomsBaseUri}/{roomId}/mark-cleaning", new { }, cancellationToken);

    public Task MarkRoomAsAvailableAsync(int roomId, CancellationToken cancellationToken = default) =>
        PostAsync<object>($"{RoomsBaseUri}/{roomId}/mark-available", new { }, cancellationToken);

    public async Task<List<Triage>> GetTriagesAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<Triage>>(TriagesBaseUri, cancellationToken) ?? new List<Triage>();

    public async Task<Triage?> GetTriageByVisitIdAsync(int visitId, CancellationToken cancellationToken = default) =>
        (await GetTriagesAsync(cancellationToken)).FirstOrDefault(triage => triage.Visit.VisitId == visitId);

    public async Task<Triage> CreateTriageAsync(Triage triage, CancellationToken cancellationToken = default) =>
        await PostAsync<Triage, Triage>(TriagesBaseUri, triage, cancellationToken)
        ?? throw new InvalidOperationException("Failed to create triage: no response from server.");

    public async Task<PerformTriageResponse> PerformTriageAsync(
        PerformTriageRequest request,
        CancellationToken cancellationToken = default) =>
        await PostAsync<PerformTriageRequest, PerformTriageResponse>(
            $"{TriagesBaseUri}/perform",
            request,
            cancellationToken)
        ?? throw new InvalidOperationException("Failed to perform triage: no response from server.");

    public async Task<List<TriageParameters>> GetTriageParametersAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<TriageParameters>>(TriageParametersBaseUri, cancellationToken) ?? new List<TriageParameters>();

    public Task<TriageParameters?> GetTriageParametersByTriageIdAsync(int triageId, CancellationToken cancellationToken = default) =>
        GetAsync<TriageParameters>($"{TriageParametersBaseUri}/triage/{triageId}", cancellationToken);

    public async Task<TriageParameters> CreateTriageParametersAsync(
        TriageParameters parameters,
        CancellationToken cancellationToken = default) =>
        await PostAsync<TriageParameters, TriageParameters>(TriageParametersBaseUri, parameters, cancellationToken)
        ?? throw new InvalidOperationException("Failed to create triage parameters: no response from server.");

    public async Task<List<ERVisit>> GetEligibleExaminationVisitsAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<ERVisit>>($"{ExaminationsBaseUri}/eligible-visits", cancellationToken) ?? new List<ERVisit>();

    public async Task<List<Examination>> GetExaminationsByVisitIdAsync(int visitId, CancellationToken cancellationToken = default) =>
        await GetAsync<List<Examination>>($"{ExaminationsBaseUri}/visit/{visitId}", cancellationToken) ?? new List<Examination>();

    public async Task<List<Examination>> GetPatientExaminationHistoryAsync(string patientId, CancellationToken cancellationToken = default) =>
        await GetAsync<List<Examination>>($"{ExaminationsBaseUri}/patient-history/{patientId}", cancellationToken) ?? new List<Examination>();

    public Task<ERExaminationSummary?> GetExaminationSummaryAsync(int visitId, CancellationToken cancellationToken = default) =>
        GetAsync<ERExaminationSummary>($"{ExaminationsBaseUri}/summary/{visitId}", cancellationToken);

    public async Task<Examination> CreateExaminationAsync(Examination examination, CancellationToken cancellationToken = default) =>
        await PostAsync<Examination, Examination>(ExaminationsBaseUri, examination, cancellationToken)
        ?? throw new InvalidOperationException("Failed to create examination: no response from server.");

    public Task UpdateExaminationAsync(int examId, Examination examination, CancellationToken cancellationToken = default) =>
        PutAsync($"{ExaminationsBaseUri}/{examId}", examination, cancellationToken);

    public async Task<List<ERTransferEligibleVisit>> GetEligibleTransferVisitsAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<ERTransferEligibleVisit>>($"{TransferLogsBaseUri}/eligible-visits", cancellationToken) ?? new List<ERTransferEligibleVisit>();

    public async Task<List<TransferLog>> GetTransferLogsByVisitIdAsync(int visitId, CancellationToken cancellationToken = default) =>
        await GetAsync<List<TransferLog>>($"{TransferLogsBaseUri}/visit/{visitId}", cancellationToken) ?? new List<TransferLog>();
}

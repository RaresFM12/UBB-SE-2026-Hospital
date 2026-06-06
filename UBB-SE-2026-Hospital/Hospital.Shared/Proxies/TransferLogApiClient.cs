using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class TransferLogApiClient(HttpClient httpClient) : ApiClientBase(httpClient), ITransferLogService
{
    private const string BaseUri = "api/transfer-logs";

    public async Task<List<TransferLog>> GetAllAsync()
        => await GetAsync<List<TransferLog>>(BaseUri) ?? [];

    public async Task<TransferLog?> GetByIdAsync(int id)
        => await GetAsync<TransferLog>($"{BaseUri}/{id}");

    public async Task<TransferLog> CreateAsync(TransferLog transferLog)
        => await PostAsync<TransferLog, TransferLog>(BaseUri, transferLog) ?? transferLog;

    public async Task UpdateAsync(TransferLog transferLog)
        => await PutAsync($"{BaseUri}/{transferLog.TransferLogId}", transferLog);

    public async Task DeleteAsync(int id)
        => await DeleteAsync($"{BaseUri}/{id}");

    public async Task<List<TransferLog>> GetByVisitIdAsync(int visitId)
        => await GetAsync<List<TransferLog>>($"{BaseUri}/visit/{visitId}") ?? [];

    public async Task<List<ERTransferEligibleVisit>> GetEligibleVisitsAsync()
        => await GetAsync<List<ERTransferEligibleVisit>>($"{BaseUri}/eligible-visits") ?? [];
}



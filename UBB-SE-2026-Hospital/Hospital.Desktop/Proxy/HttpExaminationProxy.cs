using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpExaminationProxy(HttpClient httpClient) : ProxyBase(httpClient), IExaminationService
{
    private const string BaseUri = "api/examinations";

    public async Task<List<Examination>> GetAllAsync()
        => await GetAsync<List<Examination>>(BaseUri) ?? [];

    public async Task<Examination?> GetByIdAsync(int id)
        => await GetAsync<Examination>($"{BaseUri}/{id}");

    public async Task<List<Examination>> GetByVisitIdAsync(int visitId)
        => await GetAsync<List<Examination>>($"{BaseUri}/visit/{visitId}") ?? [];

    public async Task<Examination> CreateAsync(Examination examination)
        => await PostAsync<Examination, Examination>(BaseUri, examination) ?? examination;

    public async Task<Examination> UpdateAsync(Examination examination)
    {
        await PutAsync($"{BaseUri}/{examination.ExaminationId}", examination);
        return examination;
    }

    public async Task DeleteAsync(int id)
        => await DeleteAsync($"{BaseUri}/{id}");

    public async Task<List<ERVisit>> GetEligibleVisitsAsync()
        => await GetAsync<List<ERVisit>>($"{BaseUri}/eligible-visits") ?? [];

    public async Task<List<Examination>> GetPatientHistoryAsync(int patientId)
        => await GetAsync<List<Examination>>($"{BaseUri}/patient/{patientId}") ?? [];

    public async Task<ERExaminationSummary?> GetSummaryByVisitIdAsync(int visitId)
        => await GetAsync<ERExaminationSummary>($"{BaseUri}/summary/{visitId}");
}

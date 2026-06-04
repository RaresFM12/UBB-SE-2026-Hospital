using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpTransplantProxy(HttpClient httpClient) : ProxyBase(httpClient), ITransplantService
{
    private const string BaseUri = "api/transplants";

    public async Task<List<Transplant>> GetAllAsync()
        => await GetAsync<List<Transplant>>(BaseUri) ?? [];

    public async Task<Transplant?> GetByIdAsync(int id)
        => await GetAsync<Transplant>($"{BaseUri}/{id}");

    public async Task<List<Transplant>> GetByReceiverIdAsync(int receiverId)
        => await GetAsync<List<Transplant>>($"{BaseUri}/receiver/{receiverId}") ?? [];

    public async Task<List<Transplant>> GetByDonorIdAsync(int donorId)
        => await GetAsync<List<Transplant>>($"{BaseUri}/donor/{donorId}") ?? [];

    public async Task<List<TransplantMatch>> GetTopMatchesForDonorAsync(int donorId, string organType)
        => await GetAsync<List<TransplantMatch>>($"{BaseUri}/matches/donor/{donorId}?organType={Uri.EscapeDataString(organType)}") ?? [];

    public async Task<List<TransplantMatch>> GetTopMatchesAsDisplayModelsAsync(int donorId, string organType)
        => await GetAsync<List<TransplantMatch>>($"{BaseUri}/matches/donor/{donorId}?organType={Uri.EscapeDataString(organType)}") ?? [];

    public async Task<List<TransplantMatch>> GetMatchesAsync()
        => await GetAsync<List<TransplantMatch>>($"{BaseUri}/matches") ?? [];

    public async Task<bool> IsUrgentAsync(int patientId)
        => await GetAsync<bool>($"{BaseUri}/urgent/{patientId}");

    public async Task<string?> GetChronicWarningAsync(int patientId)
        => await GetAsync<string>($"{BaseUri}/chronic-warning/{patientId}");

    public async Task CreateWaitlistRequestAsync(int receiverId, string organType)
        => await PostAsync<object, object>($"{BaseUri}/waitlist", new { ReceiverId = receiverId, OrganType = organType });

    public async Task AssignDonorAsync(int transplantId, int donorId, float finalScore)
        => await PutAsync($"{BaseUri}/{transplantId}/assign-donor", new { DonorId = donorId, FinalScore = finalScore });
}

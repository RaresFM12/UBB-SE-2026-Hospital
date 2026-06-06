using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Json;
using System.Text.Json;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class TransplantApiClient : ApiClientBase, ITransplantApiClient, ITransplantService
{
    private const string BaseUri = "api/transplants";
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new () { PropertyNameCaseInsensitive = true };

    public TransplantApiClient(HttpClient httpClient) : base(httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<Transplant?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync($"{BaseUri}/{id}", cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Transplant>(jsonOptions, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the transplant API.");
        }
    }

    public async Task<List<Transplant>> GetByReceiverIdAsync(int receiverId, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<List<Transplant>>(
                $"{BaseUri}/receiver/{receiverId}", jsonOptions, cancellationToken) ?? new List<Transplant>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the transplant API.");
        }
    }

    public async Task<List<Transplant>> GetByDonorIdAsync(int donorId, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<List<Transplant>>(
                $"{BaseUri}/donor/{donorId}", jsonOptions, cancellationToken) ?? new List<Transplant>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the transplant API.");
        }
    }

    public async Task<List<TransplantMatch>> GetTopMatchesForDonorAsync(int donorId, string organType, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<List<TransplantMatch>>(
                $"{BaseUri}/matches/donor/{donorId}?organType={Uri.EscapeDataString(organType)}",
                jsonOptions, cancellationToken) ?? new List<TransplantMatch>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the transplant API.");
        }
    }

    public async Task<bool> IsUrgentAsync(int patientId, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(
                $"{BaseUri}/urgent/{patientId}", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>(jsonOptions, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the transplant API.");
        }
    }

    public async Task<string?> GetChronicWarningAsync(int patientId, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(
                $"{BaseUri}/chronic-warning/{patientId}", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<string?>(jsonOptions, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the transplant API.");
        }
    }

    public async Task CreateWaitlistRequestAsync(int receiverId, string organType, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                $"{BaseUri}/waitlist",
                new { ReceiverId = receiverId, OrganType = organType },
                jsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string error = await ApiErrorReader.ReadErrorMessageAsync(response, cancellationToken);
                throw new InvalidOperationException(error);
            }
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the transplant API.");
        }
    }

    public async Task AssignDonorAsync(int transplantId, int donorId, float finalScore, CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PutAsJsonAsync(
                $"{BaseUri}/{transplantId}/assign-donor",
                new { DonorId = donorId, FinalScore = finalScore },
                jsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string error = await ApiErrorReader.ReadErrorMessageAsync(response, cancellationToken);
                throw new InvalidOperationException(error);
            }
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the transplant API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The transplant API request timed out.");
        }
    }

    // ITransplantService methods
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

    public async Task<Transplant> CreateAsync(Transplant transplant)
        => await PostAsync<Transplant, Transplant>(BaseUri, transplant) ?? transplant;

    public async Task<Transplant> UpdateAsync(Transplant transplant)
    {
        await PutAsync($"{BaseUri}/{transplant.TransplantId}", transplant);
        return transplant;
    }

    public async Task DeleteAsync(int id)
        => await DeleteAsync($"{BaseUri}/{id}");

    public async Task<List<Transplant>> GetByPatientIdAsync(int patientId)
        => await GetAsync<List<Transplant>>($"{BaseUri}/patient/{patientId}") ?? [];
}

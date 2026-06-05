using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class TriageDecisionApiClient(HttpClient httpClient) : ApiClientBase(httpClient), ITriageDecisionService, ITriageDecisionApiClient
{
    private const string BaseUri = "api/triage-decision";

    public int CalculateTriageLevel(TriageParameters parameters)
        => Task.Run(async () => await PostAsync<TriageParameters, int>($"{BaseUri}/level", parameters)).GetAwaiter().GetResult();

    public string DetermineSpecialization(TriageParameters parameters)
        => Task.Run(async () => await PostAsync<TriageParameters, string>($"{BaseUri}/specialization", parameters) ?? string.Empty).GetAwaiter().GetResult();
}

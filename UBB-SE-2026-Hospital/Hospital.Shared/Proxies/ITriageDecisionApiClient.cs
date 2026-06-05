using Hospital.Data.Models;

namespace Hospital.Shared.Proxies;

public interface ITriageDecisionApiClient
{
    int CalculateTriageLevel(TriageParameters parameters);
    string DetermineSpecialization(TriageParameters parameters);
}

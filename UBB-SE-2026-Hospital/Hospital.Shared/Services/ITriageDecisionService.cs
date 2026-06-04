using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface ITriageDecisionService
{
    int CalculateTriageLevel(TriageParameters parameters);
    string DetermineSpecialization(TriageParameters parameters);
}

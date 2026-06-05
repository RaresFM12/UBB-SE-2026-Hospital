using Hospital.Data.Models;
using Hospital.Services.PatientEr.Strategies;
using Hospital.Shared.Services;

namespace Hospital.Services.PatientEr;

public class TriageDecisionService(ITriageAlgorithm algorithm) : ITriageDecisionService
{
    private ITriageAlgorithm _algorithm = algorithm;

    public void SetAlgorithm(ITriageAlgorithm algorithm) => _algorithm = algorithm;

    public int CalculateTriageLevel(TriageParameters parameters) => _algorithm.CalculateTriageLevel(parameters);

    public string DetermineSpecialization(TriageParameters parameters) => _algorithm.DetermineSpecialization(parameters);
}

using Hospital.Data.Models;

namespace Hospital.Services.PatientEr.Strategies;

public interface ITriageAlgorithm
{
    int CalculateTriageLevel(TriageParameters parameters);
    string DetermineSpecialization(TriageParameters parameters);
}

using Hospital.Data.Models;

namespace Hospital.Services.PatientEr.Strategies;

public class StandardTriageAlgorithm : ITriageAlgorithm
{
    public int CalculateTriageLevel(TriageParameters parameters)
    {
        parameters.ValidateParameters();

        if (parameters.Consciousness == 3
            || parameters.Breathing == 3
            || parameters.InjuryType == 3
            || parameters.Bleeding == 3)
        {
            return 1;
        }

        int severityScore =
            (parameters.Consciousness * 3)
            + (parameters.Breathing * 3)
            + (parameters.Bleeding * 2)
            + (parameters.InjuryType * 2)
            + parameters.PainLevel;

        if (severityScore >= 20) return 2;
        if (severityScore >= 16) return 3;
        if (severityScore >= 12) return 4;

        return 5;
    }

    public string DetermineSpecialization(TriageParameters parameters)
    {
        parameters.ValidateParameters();

        if (parameters.Bleeding == 3 || parameters.InjuryType == 3) return "General Surgery";
        if (parameters.InjuryType == 2) return "Orthopedics";
        if (parameters.Breathing == 2) return "Pulmonology";
        if (parameters.Consciousness is 2 or 3) return "Neurology";

        return "Emergency Medicine";
    }
}

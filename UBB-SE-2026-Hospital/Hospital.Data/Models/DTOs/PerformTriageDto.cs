using System;
using Hospital.Data.Models;

namespace Hospital.Data.Models.DTOs;

public class PerformTriageRequest
{
    public int VisitId { get; set; }
    public int TriageLevel { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public int NurseId { get; set; }
    public DateTime TriageTime { get; set; } = DateTime.Now;
    public int Consciousness { get; set; }
    public int Breathing { get; set; }
    public int Bleeding { get; set; }
    public int InjuryType { get; set; }
    public int PainLevel { get; set; }

    public TriageParameters ToParameters(Triage triage) =>
        new()
        {
            Triage = triage,
            Consciousness = Consciousness,
            Breathing = Breathing,
            Bleeding = Bleeding,
            InjuryType = InjuryType,
            PainLevel = PainLevel
        };
}

public class PerformTriageResponse
{
    public Triage Triage { get; set; } = new();
    public TriageParameters Parameters { get; set; } = new();
}

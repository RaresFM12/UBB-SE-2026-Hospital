using System;

namespace Hospital.Data.Models;

public class Triage
{
    private const int DefaultTriageLevel = 5;

    public int TriageId { get; set; }
    public ERVisit Visit { get; set; } = null!;
    public int TriageLevel { get; set; } = DefaultTriageLevel;
    public string Specialization { get; set; } = string.Empty;
    public int NurseId { get; set; }
    public DateTime TriageTime { get; set; } = DateTime.Now;
}

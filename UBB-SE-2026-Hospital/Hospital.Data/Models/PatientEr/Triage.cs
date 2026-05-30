using System;

namespace Hospital.Data.Models;

public class Triage
{
    public int TriageId { get; set; }
    public int VisitId { get; set; }
    public int TriageLevel { get; set; } = 5;
    public string Specialization { get; set; } = string.Empty;
    public int NurseId { get; set; }
    public DateTime TriageTime { get; set; } = DateTime.Now;
}

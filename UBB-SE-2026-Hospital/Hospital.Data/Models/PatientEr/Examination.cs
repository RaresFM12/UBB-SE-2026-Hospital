using System;
using System.Collections.Generic;

namespace Hospital.Data.Models;

public class Examination
{
    public int ExaminationId { get; set; }
    public ERVisit Visit { get; set; } = null!;
    public Staff Doctor { get; set; } = null!;
    public DateTime ExaminationDate { get; set; } = DateTime.Now;
    public ERRoom Room { get; set; } = null!;
    public string Findings { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;

    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();
        if (Visit is null) errors.Add("Visit is required.");
        if (Doctor is null) errors.Add("Doctor is required.");
        if (Room is null) errors.Add("Room is required.");
        if (string.IsNullOrWhiteSpace(Findings)) errors.Add("Findings must not be empty.");
        if (ExaminationDate == default) errors.Add("Examination date and time is required.");
        return errors.Count == 0;
    }
}

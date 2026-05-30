using System;
using System.Collections.Generic;

namespace Hospital.Data.Models;

public class Examination
{
    public int ExaminationId { get; set; }
    public int VisitId { get; set; }
    public int DoctorId { get; set; }
    public DateTime ExaminationDate { get; set; } = DateTime.Now;
    public int RoomId { get; set; }
    public string Findings { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;

    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();
        if (VisitId <= 0) errors.Add("VisitId is required.");
        if (DoctorId <= 0) errors.Add("DoctorId is required.");
        if (RoomId <= 0) errors.Add("RoomId is required.");
        if (string.IsNullOrWhiteSpace(Findings)) errors.Add("Findings must not be empty.");
        if (ExaminationDate == default) errors.Add("Examination date and time is required.");
        return errors.Count == 0;
    }
}

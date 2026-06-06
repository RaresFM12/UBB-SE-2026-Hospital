using System;

namespace Hospital.Data.Models;

public class PeriodTrackerNoteDto
{
    public int NoteId { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsDone { get; set; }
}

public class UpdatePeriodTrackerRequest
{
    public DateTimeOffset StartPeriodDate { get; set; }
    public double CycleDays { get; set; }
    public double PeriodLasts { get; set; }
    public int PremenstrualSyndromeOption { get; set; }
}

public class AddPeriodTrackerNoteRequest
{
    public string NoteBody { get; set; } = string.Empty;
}

public class UpdatePeriodTrackerNoteRequest
{
    public string NoteBody { get; set; } = string.Empty;
    public bool IsDone { get; set; }
}

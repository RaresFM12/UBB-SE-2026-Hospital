using System;

namespace Hospital.Data.Models;

public class ERRequest
{
    public const string PendingStatus = "PENDING";

    public int Id { get; set; }
    public int? VisitId { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = PendingStatus;
    public ERVisit? Visit { get; set; }
    public Doctor? AssignedDoctor { get; set; }
}

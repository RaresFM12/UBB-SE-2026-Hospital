using Hospital.Data.Models;

namespace Hospital.Data.Models.DTOs;

public class ERRoomVisitDetails
{
    public ERVisit? Visit { get; set; }
    public Patient? Patient { get; set; }
    public Triage? Triage { get; set; }
}

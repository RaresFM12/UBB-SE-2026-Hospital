using System;
using System.Collections.Generic;
using System.Linq;

namespace Hospital.Data.Models;

public class ERVisit
{
    public int VisitId { get; set; }
    public int PatientId { get; set; }
    public DateTime ArrivalDateTime { get; set; } = DateTime.Now;
    public string ChiefComplaint { get; set; } = string.Empty;
    public string Status { get; set; } = VisitStatus.REGISTERED;

    public static class VisitStatus
    {
        public const string REGISTERED = "REGISTERED";
        public const string TRIAGED = "TRIAGED";
        public const string WAITING_FOR_ROOM = "WAITING_FOR_ROOM";
        public const string IN_ROOM = "IN_ROOM";
        public const string WAITING_FOR_DOCTOR = "WAITING_FOR_DOCTOR";
        public const string IN_EXAMINATION = "IN_EXAMINATION";
        public const string TRANSFERRED = "TRANSFERRED";
        public const string CLOSED = "CLOSED";
    }

    public static readonly IReadOnlyList<string> AllowedStatuses = new[]
    {
        VisitStatus.REGISTERED, VisitStatus.TRIAGED, VisitStatus.WAITING_FOR_ROOM,
        VisitStatus.IN_ROOM, VisitStatus.WAITING_FOR_DOCTOR, VisitStatus.IN_EXAMINATION,
        VisitStatus.TRANSFERRED, VisitStatus.CLOSED,
    };

    public static readonly Dictionary<string, List<string>> ValidTransitions = new()
    {
        { VisitStatus.REGISTERED,         new List<string> { VisitStatus.TRIAGED } },
        { VisitStatus.TRIAGED,            new List<string> { VisitStatus.WAITING_FOR_ROOM, VisitStatus.CLOSED } },
        { VisitStatus.WAITING_FOR_ROOM,   new List<string> { VisitStatus.IN_ROOM } },
        { VisitStatus.IN_ROOM,            new List<string> { VisitStatus.WAITING_FOR_DOCTOR } },
        { VisitStatus.WAITING_FOR_DOCTOR, new List<string> { VisitStatus.IN_EXAMINATION } },
        { VisitStatus.IN_EXAMINATION,     new List<string> { VisitStatus.TRANSFERRED, VisitStatus.CLOSED } },
        { VisitStatus.TRANSFERRED,        new List<string>() },
        { VisitStatus.CLOSED,             new List<string>() },
    };

    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();
        if (PatientId <= 0) errors.Add("PatientId is required.");
        if (ArrivalDateTime == default) errors.Add("Arrival date and time is required.");
        if (string.IsNullOrWhiteSpace(ChiefComplaint)) errors.Add("Chief complaint is required.");
        else if (ChiefComplaint.Length > 500) errors.Add("Chief complaint must not exceed 500 characters.");
        if (!AllowedStatuses.Contains(Status))
            errors.Add($"Invalid status '{Status}'. Must be one of: {string.Join(", ", AllowedStatuses)}.");
        return errors.Count == 0;
    }
}

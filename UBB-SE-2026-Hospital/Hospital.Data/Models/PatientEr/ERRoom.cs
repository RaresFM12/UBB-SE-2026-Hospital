using System;
using System.Collections.Generic;
using System.Linq;

namespace Hospital.Data.Models;

public class ERRoom
{
    public int RoomId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public string AvailabilityStatus { get; set; } = RoomStatus.Available;
    public ERVisit? CurrentVisit { get; set; }

    public static class RoomStatus
    {
        public const string Available = "Available";
        public const string Occupied = "Occupied";
        public const string Cleaning = "Cleaning";
    }

    public static class RoomType
    {
        public const string OperatingRoom = "Operating Room (OR)";
        public const string TraumaBay = "Trauma/Resuscitation Bay";
        public const string RespiratoryRoom = "Respiratory/Monitored Room";
        public const string NeurologyRoom = "Neurology/Quiet Observation Room";
        public const string OrthopedicRoom = "Orthopedic/Procedure Room";
        public const string GeneralRoom = "General Examination Room";
    }

    public static readonly IReadOnlyList<string> AllowedStatuses = new[]
    {
        RoomStatus.Available, RoomStatus.Occupied, RoomStatus.Cleaning,
    };

    private static readonly Dictionary<string, string> ValidTransitions = new()
    {
        { RoomStatus.Available, RoomStatus.Occupied },
        { RoomStatus.Occupied,  RoomStatus.Cleaning },
        { RoomStatus.Cleaning,  RoomStatus.Available },
    };

    public static bool StatusEquals(string? left, string? right)
        => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    public static string NormalizeStatus(string? status)
    {
        bool MatchesCurrentStatus(string allowedStatus) => StatusEquals(allowedStatus, status);
        return AllowedStatuses.FirstOrDefault(MatchesCurrentStatus) ?? status ?? string.Empty;
    }

    public void UpdateAvailabilityStatus(string newStatus)
    {
        string current = NormalizeStatus(AvailabilityStatus);
        string next = NormalizeStatus(newStatus);

        bool MatchesNextStatus(string allowedStatus) => StatusEquals(allowedStatus, next);

        if (!AllowedStatuses.Any(MatchesNextStatus))
            throw new ArgumentException($"'{newStatus}' is not a valid room status.");

        if (!ValidTransitions.TryGetValue(current, out string? expectedNext) || !StatusEquals(expectedNext, next))
            throw new InvalidOperationException(
                $"Invalid room status transition: cannot move Room {RoomId} from '{AvailabilityStatus}' to '{newStatus}'.");

        AvailabilityStatus = next;
    }

    public override string ToString() => $"[Room {RoomId}] Type: {RoomTypeName} | Status: {AvailabilityStatus}";
}

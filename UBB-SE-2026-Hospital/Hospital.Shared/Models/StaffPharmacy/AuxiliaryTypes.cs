namespace Hospital.Shared.Models.StaffPharmacy;

/// <summary>Represents the outcome of automatically dispatching an ER request to a doctor.</summary>
public class ERDispatchResult
{
    public int RequestId { get; set; }

    public bool Success { get; set; }

    public int? AssignedDoctorId { get; set; }

    public string? AssignedDoctorName { get; set; }

    public string Message { get; set; } = string.Empty;
}

/// <summary>Summary of the automated weekly fatigue-audit outcome.</summary>
public class AutoAuditResult
{
    public int WeeklyHoursThreshold { get; set; }

    public IReadOnlyList<int> FatiguedStaffIds { get; set; } = [];

    public int ReassignedShifts { get; set; }

    public string Summary { get; set; } = string.Empty;
}

/// <summary>Lightweight doctor profile used by ER dispatch candidate lists.</summary>
public class DoctorProfile
{
    public int DoctorId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }
}

/// <summary>Represents the current algorithmic state of the period-tracker for one user.</summary>
public class PeriodTrackerState
{
    public DateTimeOffset? LastPeriodStart { get; set; }

    public double CycleLengthDays { get; set; }

    public double PeriodLengthDays { get; set; }

    public int PremenstrualSyndromeOption { get; set; }
}

/// <summary>A month-level snapshot used by the period-tracker dashboard view.</summary>
public class PeriodTrackerDashboardSnapshot
{
    public int Year { get; set; }

    public int Month { get; set; }

    public IReadOnlyList<DateOnly> PeriodDays { get; set; } = [];

    public IReadOnlyList<DateOnly> FertileWindowDays { get; set; } = [];

    public IReadOnlyList<DateOnly> PmsDays { get; set; } = [];

    public DateOnly? PredictedNextPeriodStart { get; set; }
}

/// <summary>A user basket entry associating an item with a quantity and an optional extra discount.</summary>
public class BasketEntry
{
    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public float ExtraDiscountPercentage { get; set; }
}

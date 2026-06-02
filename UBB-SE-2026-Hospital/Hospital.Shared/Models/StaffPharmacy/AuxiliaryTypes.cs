namespace Hospital.Shared.Models.StaffPharmacy;

public class ERDispatchResult
{
    public int RequestId { get; set; }

    public bool Success { get; set; }
    public bool IsSuccess
    {
        get => Success;
        set => Success = value;
    }

    public int? AssignedDoctorId { get; set; }

    public string? AssignedDoctorName { get; set; }
    public string? MatchedDoctorName
    {
        get => AssignedDoctorName;
        set => AssignedDoctorName = value;
    }

    public string Message { get; set; } = string.Empty;
    public string MatchReason
    {
        get => Message;
        set => Message = value;
    }

    public Hospital.Data.Models.ERRequest? Request { get; set; }
}

public class AutoAuditResult
{
    public int WeeklyHoursThreshold { get; set; }

    public IReadOnlyList<Hospital.Data.Models.AuditViolation> Violations { get; set; } = [];
    public IReadOnlyList<int> FatiguedStaffIds
    {
        get => Violations.Select(violation => violation.StaffId).ToList();
        set => Violations = value.Select(staffId => new Hospital.Data.Models.AuditViolation { StaffId = staffId }).ToList();
    }

    public int ReassignedShifts { get; set; }

    public string Summary { get; set; } = string.Empty;
    public IReadOnlyList<Hospital.Data.Models.AutoSuggestRecommendation> Suggestions { get; set; } = [];
    public bool CanPublish { get; set; }
}

public class DoctorProfile
{
    public int DoctorId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }
}

public class PeriodTrackerState
{
    public DateTimeOffset? LastPeriodStart { get; set; }
    public bool HasPeriodTracker => LastPeriodStart.HasValue;
    public DateTimeOffset StartPeriodDate
    {
        get => LastPeriodStart ?? DateTimeOffset.MinValue;
        set => LastPeriodStart = value;
    }

    public double CycleLengthDays { get; set; }
    public double CycleDays
    {
        get => CycleLengthDays;
        set => CycleLengthDays = value;
    }

    public double PeriodLengthDays { get; set; }
    public double PeriodLasts
    {
        get => PeriodLengthDays;
        set => PeriodLengthDays = value;
    }

    public int PremenstrualSyndromeOption { get; set; }
}

public class PeriodTrackerDashboardSnapshot
{
    public int Year { get; set; }

    public int Month { get; set; }

    public IReadOnlyList<DateOnly> PeriodDays { get; set; } = [];

    public IReadOnlyList<DateOnly> FertileWindowDays { get; set; } = [];

    public IReadOnlyList<DateOnly> PmsDays { get; set; } = [];

    public DateOnly? PredictedNextPeriodStart { get; set; }
    public bool HasPeriodTracker { get; set; }
    public DateTime StartPeriodDate { get; set; }
    public int CycleDays { get; set; }
    public int PeriodLasts { get; set; }
    public int PMSOption { get; set; }
    public int MonthOffset { get; set; }
    public string CurrentMonthName { get; set; } = string.Empty;
    public string PeriodIntervalText { get; set; } = string.Empty;
    public string LowFertilityIntervalText { get; set; } = string.Empty;
    public string OvulationIntervalText { get; set; } = string.Empty;
    public string PmsIntervalText { get; set; } = string.Empty;
    public string CurrentPhaseString { get; set; } = string.Empty;
    public string NextPeriodDateString { get; set; } = string.Empty;
    public string NextPeriodDistanceString { get; set; } = string.Empty;
    public bool IsInMenstrualPhase { get; set; }
    public int CurrentDayOfCycle { get; set; }
    public int DaysUntilOvulation { get; set; }
    public string OvulationDistanceString { get; set; } = string.Empty;
    public IReadOnlyList<PeriodTrackerNoteSnapshot> Notes { get; set; } = [];
    public IReadOnlyList<PeriodTrackerShopItemSnapshot> ShopItems { get; set; } = [];
}

public class PeriodTrackerNoteSnapshot
{
    public int NoteId { get; set; }
    public string NoteBody { get; set; } = string.Empty;
    public bool IsDone { get; set; }
}

public class PeriodTrackerShopItemSnapshot
{
    public Hospital.Data.Models.Item? RawItem { get; set; }
    public float DisplayPrice { get; set; }
    public bool HasDiscountApplied { get; set; }
}

public class BasketEntry
{
    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public float ExtraDiscountPercentage { get; set; }
}

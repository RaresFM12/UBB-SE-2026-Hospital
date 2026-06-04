namespace Hospital.Shared.Services
{
    using System;

    public class PeriodTrackerState
    {
        public DateTimeOffset StartPeriodDate { get; set; }

        public int CycleDays { get; set; }

        public int PeriodLasts { get; set; }

        public int PremenstrualSyndromeOption { get; set; }

        public bool HasPeriodTracker { get; set; }
    }
}

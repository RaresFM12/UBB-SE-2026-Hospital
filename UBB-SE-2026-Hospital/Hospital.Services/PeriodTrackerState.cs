using System;
using System.Collections.Generic;
using System.Text;

namespace Hospital.Services
{
    public class PeriodTrackerState
    {
        public DateTimeOffset StartPeriodDate { get; set; }

        public int CycleDays { get; set; }

        public int PeriodLasts { get; set; }

        public int PremenstrualSyndromeOption { get; set; }

        public bool HasPeriodTracker { get; set; }
    }
}

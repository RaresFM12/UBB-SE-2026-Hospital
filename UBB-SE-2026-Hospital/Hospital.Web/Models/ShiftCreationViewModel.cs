namespace Hospital.Web.Models
{
    using System;
    using System.Collections.Generic;
    using Hospital.Data.Models;

    public class ShiftCreationViewModel
    {
        public string? SelectedLocation { get; set; }

        public string? SelectedQualification { get; set; }

        public int? SelectedStaffId { get; set; }

        public DateTime? ShiftDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public IReadOnlyList<string> Locations { get; init; } = new List<string>();

        public IReadOnlyList<string> Qualifications { get; init; } = new List<string>();

        public IReadOnlyList<IStaff> QualifiedStaff { get; init; } = new List<IStaff>();

        public IReadOnlyList<Shift> TodayShifts { get; init; } = new List<Shift>();
    }
}

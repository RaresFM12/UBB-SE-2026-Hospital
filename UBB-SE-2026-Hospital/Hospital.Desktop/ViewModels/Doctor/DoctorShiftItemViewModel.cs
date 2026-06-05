using Hospital.Data.Models;
namespace Hospital.Desktop.ViewModels.Doctor
{
    using System;
    

    public sealed class DoctorShiftItemViewModel
    {
        public int ShiftId { get; }

        public DateTime StartTime { get; }

        public DateTime EndTime { get; }

        public string Location { get; }

        public string Status { get; }

        public string DateText => this.StartTime.ToString("dd MMM yyyy");

        public string TimeRangeText => $"{this.StartTime:HH:mm} - {this.EndTime:HH:mm}";

        public string LocationText => string.IsNullOrWhiteSpace(this.Location) ? "Location TBD" : this.Location;

        public DoctorShiftItemViewModel(Shift shift)
        {
            this.ShiftId = shift.Id;
            this.StartTime = shift.StartTime;
            this.EndTime = shift.EndTime;
            this.Location = shift.Location ?? string.Empty;
            this.Status = shift.Status.ToString();
        }
    }
}

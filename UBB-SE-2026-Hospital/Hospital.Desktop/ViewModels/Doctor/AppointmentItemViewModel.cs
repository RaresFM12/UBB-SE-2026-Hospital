using Hospital.Data.Models;
namespace Hospital.Desktop.ViewModels.Doctor
{
    using System;
    

    public class AppointmentItemViewModel
    {
        public int AppointmentItemId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string DateText => this.Date.ToString("dd MMM yyyy");

        public string Notes { get; set; } = string.Empty;

        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string TimeRangeText => $"{this.StartTime:hh\\:mm} - {this.EndTime:hh\\:mm}";

        public string LocationSafe => string.IsNullOrWhiteSpace(this.Location) ? "Location TBD" : this.Location;

        public AppointmentItemViewModel(Appointment item)
        {
            this.AppointmentItemId = item.Id;
            this.PatientName = item.PatientName ?? string.Empty;
            this.Date = item.AppointmentDate;
            this.Notes = item.Notes ?? string.Empty;
            this.DoctorId = item.Doctor?.StaffID ?? 0;
            this.DoctorName = item.Doctor?.FullName ?? string.Empty;
            this.Type = item.Type ?? string.Empty;
            this.Location = item.Location ?? string.Empty;
            this.Status = item.Status ?? string.Empty;
            this.StartTime = item.StartTime;
            this.EndTime = item.EndTime;
        }

        public Hospital.Data.Models.Appointment ToAppointment() => new Hospital.Data.Models.Appointment
        {
            Id = this.AppointmentItemId,
            PatientName = this.PatientName,
            Doctor = new Hospital.Data.Models.Doctor { StaffID = this.DoctorId },
            AppointmentDate = this.Date,
            StartTime = this.StartTime,
            EndTime = this.EndTime,
            Status = this.Status,
            Type = this.Type,
            Location = this.Location,
            Notes = this.Notes,
        };
    }
}

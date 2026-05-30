using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Data.Models;

public class Appointment
{
    public int Id { get; set; }
    public int? DoctorId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    [NotMapped]
    public int? ExternalRefId { get; set; }

    public Doctor? Doctor { get; set; }
}

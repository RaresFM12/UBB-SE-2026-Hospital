using System;
using Hospital.Data.Models;

namespace Hospital.Data.Models.DTOs;

public class ExternalPatientDTO
{
    public string CNP { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Sex Sex { get; set; }
    public DateTime EmergencyTimestamp { get; set; }
    public string? Injury { get; set; }
}

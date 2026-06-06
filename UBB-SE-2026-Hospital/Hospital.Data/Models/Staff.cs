using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Hospital.Data.Models;

[JsonDerivedType(typeof(Staff), typeDiscriminator: "Staff")]
[JsonDerivedType(typeof(Doctor), typeDiscriminator: "Doctor")]
[JsonDerivedType(typeof(Pharmacyst), typeDiscriminator: "Pharmacist")]
public class Staff : IStaff
{
    public int StaffId { get; set; }

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
    public bool Available { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Certification { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public double HourlyRate { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<HangoutParticipant> HangoutParticipantEntries { get; set; } = new List<HangoutParticipant>();
    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<ShiftSwapRequest> ShiftSwapRequestsAsRequester { get; set; } = new List<ShiftSwapRequest>();
    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<ShiftSwapRequest> ShiftSwapRequestsAsColleague { get; set; } = new List<ShiftSwapRequest>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}

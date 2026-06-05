using Hospital.Data.Models;
using System;
using System.Collections.Generic;

namespace Hospital.Shared.Models.PatientEr;

public class Patient
{
    public int PatientId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Cnp { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public DateTime? DateOfDeath { get; set; }

    public char Sex { get; set; }

    public string PhoneNo { get; set; } = string.Empty;

    public string EmergencyContact { get; set; } = string.Empty;

    public bool IsArchived { get; set; }

    public bool IsDonor { get; set; }

    public bool Transferred { get; set; }

    public MedicalHistory? MedicalHistory { get; set; }
}
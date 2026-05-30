using System;
using System.Collections.Generic;
using Hospital.Data.Models;

namespace Hospital.Data.Models.DTOs;

public class CreatePatientRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Cnp { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Sex Sex { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public bool IsDonor { get; set; }
}

public class UpdatePatientRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Cnp { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Sex Sex { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public bool IsDonor { get; set; }
    public bool Transferred { get; set; }
    public DateTime? DateOfDeath { get; set; }
    public bool IsArchived { get; set; }
}

public class ArchiveAsDeceasedRequest
{
    public DateTime DeathDate { get; set; }
}

public class SearchPatientsRequest
{
    public string? NamePart { get; set; }
    public string? Cnp { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public Sex? Sex { get; set; }
    public bool? HasChronicCondition { get; set; }
    public DateTime? LastUpdatedFrom { get; set; }
    public DateTime? LastUpdatedTo { get; set; }
    public BloodType? BloodType { get; set; }
    public Rh? Rh { get; set; }
}

public class CreateMedicalHistoryRequest
{
    public BloodType? BloodType { get; set; }
    public Rh? Rh { get; set; }
    public List<string> ChronicConditions { get; set; } = new List<string>();
    public List<int> AllergyIds { get; set; } = new List<int>();
}

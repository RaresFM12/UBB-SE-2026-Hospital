using System;
using Hospital.Data.Models;

namespace Hospital.Data.Models.DTOs;

public class PatientFilter
{
    public string? NamePart { get; set; }
    public string? CNP { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public Sex? Sex { get; set; }
    public bool? HasChronicCondition { get; set; }
    public DateTime? LastUpdatedFrom { get; set; }
    public DateTime? LastUpdatedTo { get; set; }
    public BloodType? BloodType { get; set; }
    public Rh? Rh { get; set; }
}

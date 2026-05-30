using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Hospital.Data.Models;

public class Patient
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PatientId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(13)]
    public string Cnp { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    public DateTime? DateOfDeath { get; set; }

    [Required]
    public Sex Sex { get; set; }

    [Required]
    [MaxLength(50)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string EmergencyContact { get; set; } = string.Empty;

    [Required]
    public bool IsArchived { get; set; }

    [Required]
    public bool IsDonor { get; set; }

    [Required]
    public bool Transferred { get; set; }

    public MedicalHistory? MedicalHistory { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public bool IsDeceased => DateOfDeath.HasValue && DateOfDeath.Value > DateTime.MinValue;

    [NotMapped]
    public bool IsPoliceNotified { get; set; }

    public int GetAge()
    {
        DateTime today = DateTime.Today;
        int age = today.Year - DateOfBirth.Year;
        if (DateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }

    public bool Validate(out List<string> errors)
    {
        const int CnpLength = 13;
        const int MaxNameLength = 100;
        const int MaxPhoneLength = 50;
        const int MaxEmergencyContactLength = 200;

        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Cnp))
            errors.Add("Patient ID (CNP) is required.");
        else if (Cnp.Length != CnpLength || !Cnp.All(char.IsDigit))
            errors.Add($"Patient ID (CNP) must be exactly {CnpLength} digits.");

        if (string.IsNullOrWhiteSpace(FirstName))
            errors.Add("First name is required.");
        else if (FirstName.Length > MaxNameLength)
            errors.Add($"First name must not exceed {MaxNameLength} characters.");

        if (string.IsNullOrWhiteSpace(LastName))
            errors.Add("Last name is required.");
        else if (LastName.Length > MaxNameLength)
            errors.Add($"Last name must not exceed {MaxNameLength} characters.");

        if (DateOfBirth == default)
            errors.Add("Date of birth is required.");
        else if (DateOfBirth >= DateTime.Today)
            errors.Add("Date of birth must be in the past.");

        if (string.IsNullOrWhiteSpace(PhoneNumber))
            errors.Add("Phone number is required.");
        else if (PhoneNumber.Length > MaxPhoneLength)
            errors.Add($"Phone number must not exceed {MaxPhoneLength} characters.");

        if (string.IsNullOrWhiteSpace(EmergencyContact))
            errors.Add("Emergency contact is required.");
        else if (EmergencyContact.Length > MaxEmergencyContactLength)
            errors.Add($"Emergency contact must not exceed {MaxEmergencyContactLength} characters.");

        return errors.Count == 0;
    }
}

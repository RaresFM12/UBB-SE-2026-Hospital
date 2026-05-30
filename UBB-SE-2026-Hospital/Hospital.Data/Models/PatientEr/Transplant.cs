using System;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Data.Models;

public class Transplant
{
    [Key]
    public int TransplantId { get; set; }

    [Required]
    public Patient Receiver { get; set; } = null!;

    public Patient? Donor { get; set; }

    [Required]
    [MaxLength(100)]
    public string OrganType { get; set; } = string.Empty;

    [Required]
    public DateTime RequestDate { get; set; }

    public DateTime? TransplantDate { get; set; }

    [Required]
    public TransplantStatus Status { get; set; }

    public float CompatibilityScore { get; set; }
}

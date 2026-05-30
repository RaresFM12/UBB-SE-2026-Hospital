using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Data.Models;

public class TransplantMatch
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int TransplantId { get; set; }

    [Required]
    public int ReceiverId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ReceiverName { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string BloodType { get; set; } = string.Empty;

    [Required]
    public float CompatibilityScore { get; set; }

    [Required]
    public DateTime RequestDate { get; set; }

    [Required]
    public int WaitingDays { get; set; }
}

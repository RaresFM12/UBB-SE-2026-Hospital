using System;

namespace Hospital.Data.Models;

public class PharmacyHandover
{
    public int Id { get; set; }
    public int PharmacistId { get; set; }
    public DateTime HandoverDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Staff Pharmacist { get; set; } = null!;
}

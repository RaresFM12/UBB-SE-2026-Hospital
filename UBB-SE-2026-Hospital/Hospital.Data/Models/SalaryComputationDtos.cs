using System.Collections.Generic;

namespace Hospital.Data.Models;

public class ComputeDoctorSalaryRequest
{
    public Doctor Doctor { get; set; } = new();
    public List<Shift> MonthlyShifts { get; set; } = new();
    public int Month { get; set; }
    public int Year { get; set; }
}

public class ComputePharmacistSalaryRequest
{
    public Pharmacyst Pharmacist { get; set; } = new();
    public List<Shift> MonthlyShifts { get; set; } = new();
    public int Month { get; set; }
    public int Year { get; set; }
}

namespace Hospital.Data.Models;

public class HighRiskMedicine
{
    public int Id { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string WarningMessage { get; set; } = string.Empty;
}

namespace Hospital.Data.Models;

public class SaveExaminationRequest
{
    public int VisitId { get; set; }
    public string Notes { get; set; } = string.Empty;
}

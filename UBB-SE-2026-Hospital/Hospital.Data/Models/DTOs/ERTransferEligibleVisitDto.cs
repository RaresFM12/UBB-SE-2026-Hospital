namespace Hospital.Data.Models.DTOs;

public class ERTransferEligibleVisitDto
{
    public int VisitId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string ChiefComplaint { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool Transferred { get; set; }
}

namespace Hospital.Data.Models.DTOs;

public class CreatePrescriptionItemRequest
{
    public string MedicationName { get; set; } = string.Empty;
    public string? Quantity { get; set; }
}

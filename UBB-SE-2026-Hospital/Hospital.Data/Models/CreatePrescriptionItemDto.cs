namespace Hospital.Data.Models;

public class CreatePrescriptionItemRequest
{
    public string MedicationName { get; set; } = string.Empty;
    public string? Quantity { get; set; }
}

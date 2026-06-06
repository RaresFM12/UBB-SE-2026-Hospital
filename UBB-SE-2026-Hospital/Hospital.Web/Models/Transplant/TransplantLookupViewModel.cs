namespace Hospital.Web.Models.Transplant;

public class TransplantLookupViewModel
{
    public string PatientId { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }
}

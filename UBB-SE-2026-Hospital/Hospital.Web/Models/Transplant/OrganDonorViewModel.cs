using Hospital.Web.Models.Patients;

namespace Hospital.Web.Models.Transplant;

public class OrganDonorViewModel
{
    public string? RecipientSearchQuery { get; set; }
    public string? DonorSearchQuery { get; set; }
    public List<PatientListItemViewModel> RecipientPatients { get; set; } = new();
    public List<PatientListItemViewModel> DonorPatients { get; set; } = new();

    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public bool IsDonorRegistered { get; set; }
    public bool IsDeceased { get; set; }

    public string? SelectedOrgan { get; set; }
    public string? StatusMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public List<string> Organs { get; } = new()
    {
        "Kidney", "Heart", "Liver", "Lung", "Pancreas", "Cornea"
    };

    public List<TransplantMatchViewModel> TopMatches { get; set; } = new();
}

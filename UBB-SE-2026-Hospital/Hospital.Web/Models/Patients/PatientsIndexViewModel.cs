using Hospital.Data.Models;
using Hospital.Web.Models.Admin;

namespace Hospital.Web.Models.Patients;

public class PatientsIndexViewModel
{
    public string? SearchQuery { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public Sex? Sex { get; set; }
    public bool ShowArchived { get; set; }
    public int? SelectedPatientId { get; set; }
    public List<PatientListItemViewModel> Patients { get; set; } = new ();
    public EditPatientViewModel? SelectedPatient { get; set; }

    public bool IsActiveMode => !ShowArchived;
    public bool IsArchivedMode => ShowArchived;
}

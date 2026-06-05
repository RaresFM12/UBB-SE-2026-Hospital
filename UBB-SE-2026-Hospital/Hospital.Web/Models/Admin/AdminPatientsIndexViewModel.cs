using Hospital.Data.Models;
using Hospital.Web.Models.Patients;

namespace Hospital.Web.Models.Admin;

public class AdminPatientsIndexViewModel
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

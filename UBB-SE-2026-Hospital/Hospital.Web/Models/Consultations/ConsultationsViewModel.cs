using Hospital.Data.Models;

namespace Hospital.Web.Models.Consultations;

public class ConsultationsIndexViewModel
{
    public List<PatientOption> Patients { get; set; } = new();
    public int? SelectedPatientId { get; set; }
    public string? SelectedPatientName { get; set; }
    public List<RecordOption> Records { get; set; } = new();
}

public class PatientOption
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Cnp { get; set; } = string.Empty;
    public string Display => string.IsNullOrEmpty(Cnp) ? FullName : $"{FullName} ({Cnp})";
}

public class RecordOption
{
    public int RecordId { get; set; }
    public DateTime ConsultationDate { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
}

public class ConsultationDetailsViewModel
{
    public int RecordId { get; set; }
    public int PatientId { get; set; }
    public string PatientFirstName { get; set; } = string.Empty;
    public string PatientLastName { get; set; } = string.Empty;
    public string PatientFullName => $"{PatientFirstName} {PatientLastName}";

    public string SourceType { get; set; } = string.Empty;
    public int StaffId { get; set; }
    public DateTime ConsultationDate { get; set; }

    public string Symptoms { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;

    // Billing
    public decimal BasePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public int? DiscountApplied { get; set; }
    public bool IsDiscountApplied => DiscountApplied.HasValue;

    // Linked prescription (null when no prescription exists for this record)
    public int? PrescriptionId { get; set; }
    public bool HasPrescription => PrescriptionId.HasValue;

    // For back-navigation
    public bool IsArchived { get; set; }
}
namespace Hospital.Shared.Models.PatientEr;

public class Patient
{
    public int PatientId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Cnp { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public bool IsArchived { get; set; }
}

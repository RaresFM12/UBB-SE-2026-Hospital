namespace Hospital.Data.Models;

public class DoctorDetails
{
    public int DoctorId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;
}

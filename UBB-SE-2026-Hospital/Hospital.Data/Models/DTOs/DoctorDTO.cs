namespace Hospital.Data.Models.DTOs;

public class DoctorDetails
{
    public int DoctorId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;
}

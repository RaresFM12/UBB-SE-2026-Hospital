namespace Hospital.Shared.Models.StaffPharmacy;

public class Staff
{
    public int StaffId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = "Staff";

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;
}

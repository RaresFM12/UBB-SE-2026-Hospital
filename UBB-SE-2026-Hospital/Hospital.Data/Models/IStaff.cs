namespace Hospital.Data.Models;

public interface IStaff
{
    int StaffId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    int StaffID
    {
        get => StaffId;
        set => StaffId = value;
    }

    string FirstName { get; set; }
    string LastName { get; set; }
    string ContactInfo { get; set; }
    bool Available { get; set; }
}

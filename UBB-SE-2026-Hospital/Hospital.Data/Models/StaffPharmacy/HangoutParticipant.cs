namespace Hospital.Data.Models;

public class HangoutParticipant
{
    public int Id { get; set; }
    public int HangoutId { get; set; }
    public int StaffId { get; set; }
    public Hangout Hangout { get; set; } = null!;
    public Staff Staff { get; set; } = null!;
}

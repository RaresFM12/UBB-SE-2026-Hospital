namespace Hospital.Data.Models;

public class HangoutParticipant
{
    public int Id { get; set; }
    public Hangout Hangout { get; set; } = null!;
    public Staff Staff { get; set; } = null!;
}

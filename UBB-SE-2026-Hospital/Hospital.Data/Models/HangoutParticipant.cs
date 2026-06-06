using System.Text.Json.Serialization;

namespace Hospital.Data.Models;

public class HangoutParticipant
{
    public int Id { get; set; }
    
    [System.Text.Json.Serialization.JsonIgnore]
    public Hangout Hangout { get; set; } = null!;
    public Staff Staff { get; set; } = null!;
}

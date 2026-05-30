namespace Hospital.Data.Models;

public class PeriodNote
{
    public int Id { get; set; }
    public int NoteId { get; set; }
    public string NoteBody { get; set; } = string.Empty;
    public bool IsDone { get; set; }
    public User User { get; set; } = null!;
}

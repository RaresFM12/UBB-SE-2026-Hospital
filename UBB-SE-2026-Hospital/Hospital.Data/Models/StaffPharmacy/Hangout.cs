using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Data.Models;

public class Hangout
{
    public int HangoutID { get; set; }
    public Staff? Organizer { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }

    public string FormattedDate => Date.ToString("dd/MM/yyyy HH:mm");

    [NotMapped]
    public List<IStaff> ParticipantList { get; set; } = new();

    public ICollection<HangoutParticipant> HangoutParticipantEntries { get; set; } = new List<HangoutParticipant>();
}

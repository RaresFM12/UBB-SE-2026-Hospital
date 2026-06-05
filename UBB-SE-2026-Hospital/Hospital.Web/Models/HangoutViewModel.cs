namespace Hospital.Web.Models
{
    public class HangoutViewModel
    {
        public int HangoutId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string FormattedDate { get; set; } = string.Empty;

        public int ParticipantCount { get; set; }

        public int MaxParticipants { get; set; }

        public bool IsFull { get; set; }

        public bool IsAlreadyJoined { get; set; }

        public HashSet<int> ParticipantStaffIds { get; set; } = new HashSet<int>();
    }
}

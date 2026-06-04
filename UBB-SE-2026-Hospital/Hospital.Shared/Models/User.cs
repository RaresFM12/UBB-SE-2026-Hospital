using Hospital.Shared.Enums;

namespace Hospital.Shared.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Client;

    public DateOnly StartPeriodDate { get; set; }
    public int CycleDays { get; set; }
    public int PeriodLasts { get; set; }
    public int PremenstrualSyndromeOption { get; set; }
    public List<Hospital.Data.Models.PeriodNote> PeriodNoteEntries { get; set; } = new();
}

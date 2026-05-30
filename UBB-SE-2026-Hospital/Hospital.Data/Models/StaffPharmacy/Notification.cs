using System;

namespace Hospital.Data.Models;

public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ActionButtonText { get; set; } = string.Empty;
    public Staff? Recipient { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }

    public Notification() { }

    public Notification(string title, string message, string actionButtonText, Staff recipient)
    {
        Title = title;
        Message = message;
        ActionButtonText = actionButtonText;
        Recipient = recipient;
        CreatedAt = DateTime.UtcNow;
        IsRead = false;
    }
}

namespace Hospital.Data.Models;

public class UserNotification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ItemId { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsStockAlert { get; set; }
    public string Message { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public Item Item { get; set; } = null!;
}

namespace Hospital.Data.Models;

public class UserDiscount
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ItemId { get; set; }
    public float DiscountPercentage { get; set; }
    public User User { get; set; } = null!;
    public Item Item { get; set; } = null!;
}

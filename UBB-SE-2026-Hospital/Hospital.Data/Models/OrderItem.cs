namespace Hospital.Data.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderQuantity { get; set; }
    public float Price { get; set; }
    public Order Order { get; set; } = null!;
    public Item Item { get; set; } = null!;
}

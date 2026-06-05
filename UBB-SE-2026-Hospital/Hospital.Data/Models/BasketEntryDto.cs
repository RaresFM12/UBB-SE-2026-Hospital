namespace Hospital.Data.Models;

public class BasketEntryDto
{
    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public float ExtraDiscountPercentage { get; set; }
}

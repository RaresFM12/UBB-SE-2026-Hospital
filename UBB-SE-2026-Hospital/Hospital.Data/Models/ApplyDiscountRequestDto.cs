namespace Hospital.Data.Models;

public sealed class ApplyDiscountRequest
{
    public decimal BasePrice { get; set; }

    public int Discount { get; set; }
}

namespace Hospital.Data.Models.DTOs;

public sealed class ApplyDiscountRequest
{
    public decimal BasePrice { get; set; }

    public int Discount { get; set; }
}

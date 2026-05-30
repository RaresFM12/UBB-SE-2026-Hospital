namespace Hospital.Data.Models.DTOs;

public sealed class ApplyDiscountRequestDto
{
    public decimal BasePrice { get; set; }

    public int Discount { get; set; }
}
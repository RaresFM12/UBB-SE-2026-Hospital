namespace Hospital.Shared.Models.StaffPharmacy;

public class Item
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Producer { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public double Price { get; set; }

    public int Quantity { get; set; }
}

using System;

namespace Hospital.Data.Models;

public class ItemBatch
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public int NumberOfPacks { get; set; }
    public Item Item { get; set; } = null!;
}

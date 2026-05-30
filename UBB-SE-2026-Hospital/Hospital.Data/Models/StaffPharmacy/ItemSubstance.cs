namespace Hospital.Data.Models;

public class ItemSubstance
{
    public int Id { get; set; }
    public float Concentration { get; set; }
    public Item Item { get; set; } = null!;
    public Substance Substance { get; set; } = null!;
}

using System.Collections.Generic;

namespace Hospital.Data.Models;

public class Substance
{
    public string Name { get; set; } = string.Empty;
    public float LethalDose { get; set; }
    public string Description { get; set; } = string.Empty;

    public ICollection<ItemSubstance> ItemSubstanceEntries { get; set; } = new List<ItemSubstance>();

    public Substance() { }

    public Substance(string name, float lethalDose, string description = "")
    {
        Name = name;
        LethalDose = lethalDose;
        Description = description;
    }
}

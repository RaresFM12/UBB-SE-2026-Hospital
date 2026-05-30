using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace Hospital.Data.Models;

public class Item
{
    private const string ImagePathDefault = "..\\..\\Assets\\placeholder.png";

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Producer { get; set; } = string.Empty;
    public float Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string ImagePath { get; set; } = ImagePathDefault;
    public int NumberOfPills { get; set; }
    public int Quantity { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public float DiscountPercentage { get; set; }

    [NotMapped]
    public Dictionary<string, float> ActiveSubstances { get; set; } = new();
    [NotMapped]
    public Dictionary<DateOnly, int> Batches { get; set; } = new();

    [JsonIgnore]
    public ICollection<ItemSubstance> ItemSubstanceEntries { get; set; } = new List<ItemSubstance>();
    [JsonIgnore]
    public ICollection<ItemBatch> ItemBatchEntries { get; set; } = new List<ItemBatch>();

    public Item() { }

    public Item(int id, string name, string producer, string category,
                float price, int numberOfPills,
                string label = "", string description = "", string imagePath = ImagePathDefault,
                float discount = 0f, int quantity = 0)
    {
        Id = id;
        Name = name;
        Producer = producer;
        Price = price;
        NumberOfPills = numberOfPills;
        Category = category;
        ImagePath = imagePath;
        Quantity = quantity;
        Label = label;
        Description = description;
        DiscountPercentage = discount;
    }

    public Item(string name, string producer, string category,
                float price, int numberOfPills,
                int quantity = 0,
                string label = "", string description = "", string imagePath = ImagePathDefault,
                float discount = 0f)
        : this(0, name, producer, category, price, numberOfPills, label, description, imagePath, discount, quantity) { }

    public void AddActiveSubstanceToItem(string substanceName, float concentration)
    {
        if (ActiveSubstances.ContainsKey(substanceName))
            throw new ArgumentException($"{substanceName} is already inside the medication");
        ActiveSubstances[substanceName] = concentration;
    }

    public void ChangeActiveSubstanceConcentration(string substanceName, float newConcentration)
    {
        if (!ActiveSubstances.ContainsKey(substanceName))
            throw new ArgumentException($"{substanceName} is not inside the medication");
        ActiveSubstances[substanceName] = newConcentration;
    }

    public void RemoveActiveSubstanceFromItem(string substanceName)
    {
        if (!ActiveSubstances.ContainsKey(substanceName))
            throw new ArgumentException($"{substanceName} is not inside the medication");
        ActiveSubstances.Remove(substanceName);
    }

    public void AddNewBatchToItem(DateOnly expirationDate, int numberOfPacks)
    {
        if (Batches.ContainsKey(expirationDate))
        {
            Batches[expirationDate] += numberOfPacks;
        }
        else
        {
            Batches[expirationDate] = numberOfPacks;
        }
        Quantity += numberOfPacks;
    }

    public void ChangeNumberOfPacksForBatch(DateOnly expirationDate, int newNumberOfPacks)
    {
        if (!Batches.ContainsKey(expirationDate))
            throw new ArgumentException($"A batch with expiration date {expirationDate} doesn't exist");
        int oldNumberOfPacks = Batches[expirationDate];
        Batches[expirationDate] = newNumberOfPacks;
        Quantity += newNumberOfPacks - oldNumberOfPacks;
    }

    public void RemoveBatchFromItem(DateOnly expirationDate)
    {
        if (!Batches.ContainsKey(expirationDate))
            throw new ArgumentException($"A batch with expiration date {expirationDate} doesn't exist");
        Quantity -= Batches[expirationDate];
        Batches.Remove(expirationDate);
    }

    public void RemoveQuantityFromItem(int quantityToRemove, DateOnly dateAfter)
    {
        List<DateOnly> sorted = Batches.Keys.OrderBy(d => d).ToList();
        int remaining = quantityToRemove;
        foreach (DateOnly expiry in sorted)
        {
            if (remaining <= 0) break;
            if (expiry < dateAfter) continue;
            if (remaining >= Batches[expiry])
            {
                remaining -= Batches[expiry];
                RemoveBatchFromItem(expiry);
            }
            else
            {
                ChangeNumberOfPacksForBatch(expiry, Batches[expiry] - remaining);
                remaining = 0;
            }
        }
    }

    public int GetQuantityAtSpecifiedDate(DateOnly date)
        => Batches.Where(b => date < b.Key).Sum(b => b.Value);
}

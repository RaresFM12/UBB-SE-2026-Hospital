using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Hospital.Data.Models;

public class Order : IEquatable<Order>
{
    public const int OrderExpirationDays = 7;

    public int Id { get; set; }
    public string IdString => $"Order#{Id}";
    public DateOnly PickUpDate { get; set; }
    public string PickUpDateString => PickUpDate.ToString("yyyy.MM.dd");
    public string ExpirationDateString => PickUpDate.AddDays(OrderExpirationDays).ToString("yyyy.MM.dd");
    public bool IsCompleted { get; set; }
    public bool IsExpired { get; set; }

    [NotMapped]
    public Dictionary<int, Tuple<int, float>> ItemQuantitiesWithFinalPrice { get; set; } = new();

    [JsonIgnore]
    public User Client { get; set; } = null!;
    [JsonIgnore]
    public ICollection<OrderItem> OrderItemEntries { get; set; } = new List<OrderItem>();

    public Order() { }

    public Order(int id, User client, DateOnly pickUpDate, bool isCompleted = false, bool isExpired = false)
    {
        Id = id;
        Client = client;
        PickUpDate = pickUpDate;
        IsCompleted = isCompleted;
        IsExpired = isExpired;
    }

    public bool Equals(Order? other) => other is not null && Id == other.Id;

    public void AddItemToOrder(int newItemId, int itemQuantity, float finalPrice)
    {
        if (ItemQuantitiesWithFinalPrice.ContainsKey(newItemId))
            throw new ArgumentException($"Item #{newItemId} already exists in order");
        ItemQuantitiesWithFinalPrice[newItemId] = new Tuple<int, float>(itemQuantity, finalPrice);
    }

    public void ChangeItemInfoInOrder(int itemId, int newItemQuantity, float newFinalPrice)
    {
        if (!ItemQuantitiesWithFinalPrice.ContainsKey(itemId))
            throw new ArgumentException($"Item #{itemId} doesn't exist");
        ItemQuantitiesWithFinalPrice[itemId] = new Tuple<int, float>(newItemQuantity, newFinalPrice);
    }

    public void RemoveItemFromOrder(int itemId)
    {
        if (!ItemQuantitiesWithFinalPrice.ContainsKey(itemId))
            throw new ArgumentException($"Item #{itemId} doesn't exist");
        ItemQuantitiesWithFinalPrice.Remove(itemId);
    }
}

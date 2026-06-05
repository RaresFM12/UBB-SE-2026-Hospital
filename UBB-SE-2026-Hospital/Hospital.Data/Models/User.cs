using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Hospital.Data.Models;

public class BasketEntry
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public float ExtraDiscountPercentage { get; set; }

    public User User { get; set; } = null!;
    public Item Item { get; set; } = null!;

    public BasketEntry() { }

    public BasketEntry(int quantity, float extraDiscountPercentage = 0f)
    {
        Quantity = quantity;
        ExtraDiscountPercentage = extraDiscountPercentage;
    }
}

public class User
{
    private const string DefaultUserRole = "Client";
    private const int DefaultCycleDays = 28;
    private const int DefaultPeriodLasts = 5;

    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsDisabled { get; set; }
    public string Role { get; set; } = DefaultUserRole;
    public DateOnly StartPeriodDate { get; set; }
    public int CycleDays { get; set; } = DefaultCycleDays;
    public int PeriodLasts { get; set; } = DefaultPeriodLasts;
    public int PremenstrualSyndromeOption { get; set; }
    public bool DiscountNotifications { get; set; }
    public int LoyaltyPoints { get; set; }

    [NotMapped]
    [JsonIgnore]
    public Dictionary<int, Tuple<string, bool>> PeriodNotes { get; set; } = new();

    [NotMapped]
    [JsonIgnore]
    public List<int> StockAlerts { get; set; } = new();

    [NotMapped]
    [JsonIgnore]
    public List<int> FavoriteItems { get; set; } = new();

    [NotMapped]
    [JsonIgnore]
    public Dictionary<int, float> UserDiscounts { get; set; } = new();

    [NotMapped]
    [JsonIgnore]
    public Dictionary<int, BasketEntry> Basket { get; set; } = new();

    [JsonIgnore]
    public ICollection<PeriodNote> PeriodNoteEntries { get; set; } = new List<PeriodNote>();
    [JsonIgnore]
    public ICollection<UserDiscount> UserDiscountEntries { get; set; } = new List<UserDiscount>();
    [JsonIgnore]
    public ICollection<UserNotification> UserNotificationEntries { get; set; } = new List<UserNotification>();
    [JsonIgnore]
    public ICollection<Order> Orders { get; set; } = new List<Order>();

    [JsonConstructor]
    public User()
    {
        StartPeriodDate = DateOnly.FromDateTime(DateTime.Now);
    }

    public User(int userId, string email, string phoneNumber, string passwordHash,
                bool isAdmin, bool isDisabled, string userName,
                bool discountNotifications, int loyaltyPoints,
                DateOnly startPeriodDate = default, int cycleDays = DefaultCycleDays,
                int periodLasts = DefaultPeriodLasts, int premenstrualSyndromeOption = 0) : this()
    {
        Id = userId;
        Email = email;
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
        IsAdmin = isAdmin;
        IsDisabled = isDisabled;
        Username = userName;
        DiscountNotifications = discountNotifications;
        LoyaltyPoints = loyaltyPoints;
        StartPeriodDate = startPeriodDate.Year == default(DateOnly).Year
            ? DateOnly.FromDateTime(DateTime.Now) : startPeriodDate;
        CycleDays = cycleDays;
        PeriodLasts = periodLasts;
        PremenstrualSyndromeOption = premenstrualSyndromeOption;
    }


}

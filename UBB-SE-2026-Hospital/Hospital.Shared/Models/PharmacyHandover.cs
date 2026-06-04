namespace Hospital.Shared.Models
{
    using System;

    public class PharmacyHandover
    {
        // Surrogate primary key — replaces the old composite (PharmacistId, HandoverDate) PK.
        public int Id { get; set; }

        public DateTime HandoverDate { get; set; }

        // EF Core navigation property — persisted via shadow FK column "PharmacistId".
        public Staff Pharmacist { get; set; } = null!;
    }
}

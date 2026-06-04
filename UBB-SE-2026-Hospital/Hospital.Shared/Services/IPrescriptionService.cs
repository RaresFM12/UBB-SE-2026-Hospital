namespace Hospital.Shared.Services
{
    using System;
    using System.Collections.Generic;
    using Hospital.Shared.Models;

    public interface IPrescriptionService
    {
        Dictionary<int, int> GetItemsFromPrescription(string prescriptionId, Dictionary<int, float> userDiscounts);

        Dictionary<int, int> GetCheapestPrescriptionItems(string prescriptionName, int requiredPills);
    }
}

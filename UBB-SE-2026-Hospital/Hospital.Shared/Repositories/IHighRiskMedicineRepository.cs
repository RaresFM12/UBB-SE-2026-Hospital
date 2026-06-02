namespace Hospital.Shared.Repositories
{
    using System.Collections.Generic;

    public interface IHighRiskMedicineRepository
    {
        IReadOnlyList<(string MedicineName, string WarningMessage)> GetAllHighRiskMedicines();
    }
}

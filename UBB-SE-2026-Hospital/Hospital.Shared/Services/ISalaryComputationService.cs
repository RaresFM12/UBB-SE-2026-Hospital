using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface ISalaryComputationService
{
    Task<double> ComputeSalaryDoctorAsync(Doctor doctor, IReadOnlyList<Shift> monthlyShifts, int month, int year, CancellationToken cancellationToken = default);

    Task<double> ComputeSalaryPharmacistAsync(Pharmacyst pharmacist, IReadOnlyList<Shift> monthlyShifts, int month, int year, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Staff>> GetAllStaffAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Shift>> GetAllShiftsAsync(CancellationToken cancellationToken = default);
}

using Hospital.Shared.Models.PatientEr;

namespace Hospital.Shared.Repositories;

public interface IPatientRepository
{
    Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Patient?> GetByIdAsync(int patientId, CancellationToken cancellationToken = default);
}

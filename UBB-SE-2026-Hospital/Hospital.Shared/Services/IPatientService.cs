using Hospital.Shared.Models.PatientEr;

namespace Hospital.Shared.Services;

public interface IPatientService
{
    Task<IReadOnlyList<Patient>> GetPatientsAsync(CancellationToken cancellationToken = default);
}

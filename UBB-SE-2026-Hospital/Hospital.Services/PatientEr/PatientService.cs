using Hospital.Shared.Models.PatientEr;
using Hospital.Shared.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.PatientEr;

public class PatientService(IPatientRepository patientRepository) : IPatientService
{
    public async Task<IReadOnlyList<Patient>> GetPatientsAsync(CancellationToken cancellationToken = default)
        => await patientRepository.GetAllAsync(cancellationToken);
}

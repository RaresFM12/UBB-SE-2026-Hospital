using Hospital.Shared.Models.PatientEr;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;
using System.Linq;

namespace Hospital.Services.PatientEr;

public class PatientService(IPatientRepository patientRepository) : IPatientService
{
    public async Task<IReadOnlyList<Patient>> GetPatientsAsync(CancellationToken cancellationToken = default)
    {
        var dbPatients = await patientRepository.GetAllAsync();
        return dbPatients.Select(p => new Patient
        {
            PatientId = p.PatientId,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Cnp = p.Cnp,
            DateOfBirth = p.DateOfBirth,
            IsArchived = p.IsArchived
        }).ToList();
    }
}

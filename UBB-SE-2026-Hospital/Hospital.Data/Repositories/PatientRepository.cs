using Hospital.Shared.Models.PatientEr;
using Hospital.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class PatientRepository(HospitalDbContext context) : IPatientRepository
{
    public async Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Patients.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Patient?> GetByIdAsync(int patientId, CancellationToken cancellationToken = default)
        => await context.Patients.AsNoTracking()
            .FirstOrDefaultAsync(patient => patient.PatientId == patientId, cancellationToken);
}

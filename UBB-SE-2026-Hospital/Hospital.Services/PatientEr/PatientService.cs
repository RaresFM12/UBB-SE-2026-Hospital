using Hospital.Data.Models.DTOs;
using Hospital.Data.Repositories;
using Hospital.Shared.Models.PatientEr;
using Hospital.Shared.Services;
using System.Linq;

namespace Hospital.Services.PatientEr;

public class PatientService(IPatientRepository patientRepository) : IPatientService
{
    public Task<List<Data.Models.Patient>> SearchPatientsAsync(SearchPatientsRequest? searchCriteria, CancellationToken cancellationToken)
    {
        if (searchCriteria is null)
        {
            return patientRepository.GetAllAsync();
        }

        var filter = new PatientFilter
        {
            NamePart = searchCriteria.NamePart,
            CNP = searchCriteria.Cnp,
            MinAge = searchCriteria.MinAge,
            MaxAge = searchCriteria.MaxAge,
            Sex = searchCriteria.Sex,
            HasChronicCondition = searchCriteria.HasChronicCondition,
            LastUpdatedFrom = searchCriteria.LastUpdatedFrom,
            LastUpdatedTo = searchCriteria.LastUpdatedTo,
            BloodType = searchCriteria.BloodType,
            Rh = searchCriteria.Rh,
        };

        return patientRepository.GetFilteredAsync(filter);
    }

    public Task<Data.Models.Patient?> GetByIdAsync(int id, CancellationToken cancellationToken) => patientRepository.GetByIdAsync(id);

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
 

    public async Task<Patient> GetPatientDetailsAsync(int patientId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public async Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public async Task<List<string>> GetPatientAllergiesAsync(int patientId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public async Task<bool> IsHighRiskPatientAsync(int patientId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public async Task<RecordExportDataDto> GetRecordExportDataAsync(int recordId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public async Task<int> CreateMedicalRecordAsync(int patientId, Data.Models.MedicalRecord record, CancellationToken cancellationToken) => throw new NotImplementedException();
    public async Task CreatePrescriptionAsync(int recordId, Prescription prescription) => throw new NotImplementedException();
}

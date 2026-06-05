using Hospital.Services.PatientEr;
using Hospital.Data.Repositories;
using SharedPatient = Hospital.Shared.Models.PatientEr.Patient;
using DbPatient = Hospital.Data.Models.Patient;

namespace Hospital.Tests.Services;

[TestClass]
public class PatientServiceTests
{
    [TestMethod]
    public async Task GetPatientsAsync_ReturnsRepositoryResults()
    {
        var repository = new FakePatientRepository();
        var service = new PatientService(repository);

        var patients = await service.GetPatientsAsync();

        Assert.HasCount(1, patients);
        Assert.AreEqual("Ana", patients[0].FirstName);
    }

    private sealed class FakePatientRepository : IPatientRepository
    {
        public Task<List<DbPatient>> GetAllAsync()
            => Task.FromResult<List<DbPatient>>(
            [
                new DbPatient
                {
                    PatientId = 1,
                    FirstName = "Ana",
                    LastName = "Pop",
                },
            ]);

        public Task<DbPatient?> GetByIdAsync(int patientId)
            => Task.FromResult<DbPatient?>(null);

        public Task<List<DbPatient>> GetFilteredAsync(Hospital.Data.Models.DTOs.PatientFilter filter)
            => throw new NotImplementedException();

        public Task<DbPatient> CreateAsync(DbPatient patient)
            => throw new NotImplementedException();

        public Task<DbPatient> UpdateAsync(DbPatient patient)
            => throw new NotImplementedException();

        public Task DeleteAsync(int patientId)
            => throw new NotImplementedException();
    }
}

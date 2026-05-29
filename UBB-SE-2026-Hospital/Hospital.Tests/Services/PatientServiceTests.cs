using Hospital.Services.PatientEr;
using Hospital.Shared.Models.PatientEr;
using Hospital.Shared.Repositories;

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
        public Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Patient>>(
            [
                new Patient
                {
                    PatientId = 1,
                    FirstName = "Ana",
                    LastName = "Pop",
                },
            ]);

        public Task<Patient?> GetByIdAsync(int patientId, CancellationToken cancellationToken = default)
            => Task.FromResult<Patient?>(null);
    }
}

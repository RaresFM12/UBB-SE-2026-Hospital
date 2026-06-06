using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class PatientServiceTests
{
    private const int PatientId = 1;
    private const int RecordId = 5;
    private const int AllergyId = 7;
    private const int HighRiskRecordThreshold = 3;
    private const string FirstName = "Ana";
    private const string LastName = "Pop";
    private const string Cnp = "1234567890123";
    private const string OtherCnp = "3210987654321";
    private const string PhoneNumber = "0712345678";
    private const string EmergencyContact = "John Doe 0712345678";
    private const string AllergyName = "Penicillin";
    private const string ChronicCondition = "Asthma";
    private static readonly DateTime BirthDate = new(1990, 1, 1);
    private static readonly DateTime PastDeathDate = new(2010, 1, 1);

    private static Patient ValidPatient(int id = PatientId) => new()
    {
        PatientId = id,
        FirstName = FirstName,
        LastName = LastName,
        Cnp = Cnp,
        DateOfBirth = BirthDate,
        Sex = Sex.F,
        PhoneNumber = PhoneNumber,
        EmergencyContact = EmergencyContact,
    };

    private static CreatePatientRequest ValidCreateRequest() => new()
    {
        FirstName = FirstName,
        LastName = LastName,
        Cnp = Cnp,
        DateOfBirth = BirthDate,
        Sex = Sex.F,
        PhoneNumber = PhoneNumber,
        EmergencyContact = EmergencyContact,
    };

    private static (PatientService Service, IPatientRepository Patients, IPrescriptionRepository Prescriptions, IAllergyRepository Allergies) CreateService()
    {
        var patients = Substitute.For<IPatientRepository>();
        var prescriptions = Substitute.For<IPrescriptionRepository>();
        var allergies = Substitute.For<IAllergyRepository>();
        return (new PatientService(patients, prescriptions, allergies), patients, prescriptions, allergies);
    }

    [TestMethod]
    public async Task SearchPatientsAsync_NullCriteria_ReturnsAllPatients()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetAllAsync().Returns(new List<Patient> { ValidPatient() });

        var result = await service.SearchPatientsAsync(null, CancellationToken.None);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task SearchPatientsAsync_WithCriteria_UsesFilteredQuery()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetFilteredAsync(Arg.Any<PatientFilter>()).Returns(new List<Patient> { ValidPatient() });

        var result = await service.SearchPatientsAsync(new SearchPatientsRequest { NamePart = FirstName }, CancellationToken.None);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsRepositoryResult()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns(ValidPatient());

        var result = await service.GetByIdAsync(PatientId, CancellationToken.None);

        Assert.AreEqual(PatientId, result!.PatientId);
    }

    [TestMethod]
    public async Task GetPatientsAsync_ReturnsRepositoryResult()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetAllAsync().Returns(new List<Patient> { ValidPatient() });

        var result = await service.GetPatientsAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task CreatePatientAsync_ValidRequest_CreatesPatient()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetAllAsync().Returns(new List<Patient>());
        patients.CreateAsync(Arg.Any<Patient>()).Returns(call => (Patient)call[0]);

        var result = await service.CreatePatientAsync(ValidCreateRequest());

        Assert.AreEqual(Cnp, result.Cnp);
    }

    [TestMethod]
    public async Task CreatePatientAsync_DuplicateCnp_ThrowsArgumentException()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetAllAsync().Returns(new List<Patient> { ValidPatient() });

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreatePatientAsync(ValidCreateRequest()));
    }

    [TestMethod]
    public async Task CreatePatientAsync_InvalidData_ThrowsArgumentException()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetAllAsync().Returns(new List<Patient>());
        var request = ValidCreateRequest();
        request.Cnp = OtherCnp[..5];

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreatePatientAsync(request));
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_PatientNotFound_ThrowsArgumentException()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns((Patient?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateMedicalHistoryAsync(PatientId, new CreateMedicalHistoryRequest()));
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_AlreadyHasHistory_ThrowsArgumentException()
    {
        var (service, patients, _, _) = CreateService();
        var patient = ValidPatient();
        patient.MedicalHistory = new MedicalHistory { Patient = patient };
        patients.GetByIdAsync(PatientId).Returns(patient);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateMedicalHistoryAsync(PatientId, new CreateMedicalHistoryRequest()));
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_ValidRequest_UpdatesPatient()
    {
        var (service, patients, _, allergies) = CreateService();
        var patient = ValidPatient();
        patients.GetByIdAsync(PatientId).Returns(patient);
        allergies.GetByIdAsync(AllergyId).Returns(new Allergy { AllergyId = AllergyId, AllergyName = AllergyName });
        var request = new CreateMedicalHistoryRequest { ChronicConditions = [ChronicCondition], AllergyIds = [AllergyId] };

        await service.CreateMedicalHistoryAsync(PatientId, request);

        await patients.Received().UpdateAsync(Arg.Is<Patient>(updated => updated.MedicalHistory != null));
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_AllergyRepositoryMissing_ThrowsInvalidOperation()
    {
        var patients = Substitute.For<IPatientRepository>();
        var patient = ValidPatient();
        patients.GetByIdAsync(PatientId).Returns(patient);
        var service = new PatientService(patients);
        var request = new CreateMedicalHistoryRequest { AllergyIds = [AllergyId] };

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.CreateMedicalHistoryAsync(PatientId, request));
    }

    [TestMethod]
    public async Task GetPatientDetailsAsync_NotFound_ThrowsArgumentException()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns((Patient?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.GetPatientDetailsAsync(PatientId, CancellationToken.None));
    }

    [TestMethod]
    public async Task GetPatientDetailsAsync_Found_ReturnsPatient()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns(ValidPatient());

        var result = await service.GetPatientDetailsAsync(PatientId, CancellationToken.None);

        Assert.AreEqual(PatientId, result.PatientId);
    }

    [TestMethod]
    public async Task GetPrescriptionByRecordIdAsync_NoRepository_ReturnsNull()
    {
        var patients = Substitute.For<IPatientRepository>();
        var service = new PatientService(patients);

        var result = await service.GetPrescriptionByRecordIdAsync(RecordId, CancellationToken.None);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetPrescriptionByRecordIdAsync_WithRepository_ReturnsPrescription()
    {
        var (service, _, prescriptions, _) = CreateService();
        prescriptions.GetByRecordIdAsync(RecordId).Returns(new List<Prescription> { new() { PrescriptionId = RecordId } });

        var result = await service.GetPrescriptionByRecordIdAsync(RecordId, CancellationToken.None);

        Assert.AreEqual(RecordId, result!.PrescriptionId);
    }

    [TestMethod]
    public async Task GetPatientAllergiesAsync_NotFound_ThrowsArgumentException()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns((Patient?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.GetPatientAllergiesAsync(PatientId, CancellationToken.None));
    }

    [TestMethod]
    public async Task GetPatientAllergiesAsync_ReturnsDistinctAllergyNames()
    {
        var (service, patients, _, _) = CreateService();
        var patient = ValidPatient();
        patient.MedicalHistory = new MedicalHistory
        {
            Patient = patient,
            PatientAllergies = [new PatientAllergy { Allergy = new Allergy { AllergyName = AllergyName }, SeverityLevel = "Mild" }],
        };
        patients.GetByIdAsync(PatientId).Returns(patient);

        var result = await service.GetPatientAllergiesAsync(PatientId, CancellationToken.None);

        Assert.AreEqual(AllergyName, result[0]);
    }

    [TestMethod]
    public async Task IsHighRiskPatientAsync_NotFound_ThrowsArgumentException()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns((Patient?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.IsHighRiskPatientAsync(PatientId, CancellationToken.None));
    }

    [TestMethod]
    public async Task IsHighRiskPatientAsync_ChronicConditions_ReturnsTrue()
    {
        var (service, patients, _, _) = CreateService();
        var patient = ValidPatient();
        patient.MedicalHistory = new MedicalHistory { Patient = patient, ChronicConditions = [ChronicCondition] };
        patients.GetByIdAsync(PatientId).Returns(patient);

        bool isHighRisk = await service.IsHighRiskPatientAsync(PatientId, CancellationToken.None);

        Assert.IsTrue(isHighRisk);
    }

    [TestMethod]
    public async Task IsHighRiskPatientAsync_NoRiskFactors_ReturnsFalse()
    {
        var (service, patients, _, _) = CreateService();
        var patient = ValidPatient();
        patient.MedicalHistory = new MedicalHistory { Patient = patient };
        patients.GetByIdAsync(PatientId).Returns(patient);

        bool isHighRisk = await service.IsHighRiskPatientAsync(PatientId, CancellationToken.None);

        Assert.IsFalse(isHighRisk);
    }

    [TestMethod]
    public async Task GetRecordExportDataAsync_NotFound_ThrowsArgumentException()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetAllAsync().Returns(new List<Patient>());

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.GetRecordExportDataAsync(RecordId, CancellationToken.None));
    }

    [TestMethod]
    public async Task GetRecordExportDataAsync_Found_ReturnsRecord()
    {
        var (service, patients, _, _) = CreateService();
        var patient = ValidPatient();
        patient.MedicalHistory = new MedicalHistory
        {
            Patient = patient,
            MedicalRecords = [new MedicalRecord { RecordId = RecordId }],
        };
        patients.GetAllAsync().Returns(new List<Patient> { patient });
        patients.GetByIdAsync(PatientId).Returns(patient);

        var result = await service.GetRecordExportDataAsync(RecordId, CancellationToken.None);

        Assert.AreEqual(RecordId, result.Record.RecordId);
    }

    [TestMethod]
    public async Task UpdatePatientAsync_NotFound_ThrowsArgumentException()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns((Patient?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdatePatientAsync(ValidPatient()));
    }

    [TestMethod]
    public async Task UpdatePatientAsync_FutureDeathDate_ThrowsArgumentException()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns(ValidPatient());
        var patient = ValidPatient();
        patient.DateOfDeath = DateTime.Today.AddDays(1);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdatePatientAsync(patient));
    }

    [TestMethod]
    public async Task UpdatePatientAsync_Valid_UpdatesPatient()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns(ValidPatient());

        await service.UpdatePatientAsync(ValidPatient());

        await patients.Received().UpdateAsync(Arg.Any<Patient>());
    }

    [TestMethod]
    public async Task ArchivePatientAsync_NotFound_ThrowsArgumentException()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns((Patient?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.ArchivePatientAsync(PatientId));
    }

    [TestMethod]
    public async Task ArchivePatientAsync_Found_SetsArchived()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns(ValidPatient());

        await service.ArchivePatientAsync(PatientId);

        await patients.Received().UpdateAsync(Arg.Is<Patient>(patient => patient.IsArchived));
    }

    [TestMethod]
    public async Task DearchivePatientAsync_Deceased_ThrowsArgumentException()
    {
        var (service, patients, _, _) = CreateService();
        var patient = ValidPatient();
        patient.DateOfDeath = BirthDate.AddYears(40);
        patients.GetByIdAsync(PatientId).Returns(patient);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.DearchivePatientAsync(PatientId));
    }

    [TestMethod]
    public async Task DearchivePatientAsync_Living_ClearsArchived()
    {
        var (service, patients, _, _) = CreateService();
        var patient = ValidPatient();
        patient.IsArchived = true;
        patients.GetByIdAsync(PatientId).Returns(patient);

        await service.DearchivePatientAsync(PatientId);

        await patients.Received().UpdateAsync(Arg.Is<Patient>(updated => !updated.IsArchived));
    }

    [TestMethod]
    public async Task ArchiveAsDeceasedAsync_FutureDate_ThrowsArgumentException()
    {
        var (service, _, _, _) = CreateService();

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.ArchiveAsDeceasedAsync(PatientId, DateTime.Today.AddDays(1)));
    }

    [TestMethod]
    public async Task ArchiveAsDeceasedAsync_DeathBeforeBirth_ThrowsArgumentException()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns(ValidPatient());

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.ArchiveAsDeceasedAsync(PatientId, BirthDate.AddYears(-1)));
    }

    [TestMethod]
    public async Task ArchiveAsDeceasedAsync_Valid_SetsDateOfDeath()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns(ValidPatient());

        await service.ArchiveAsDeceasedAsync(PatientId, PastDeathDate);

        await patients.Received().UpdateAsync(Arg.Is<Patient>(patient => patient.DateOfDeath == PastDeathDate.Date));
    }

    [TestMethod]
    public async Task CreateMedicalRecordAsync_NotFound_ThrowsArgumentException()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns((Patient?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateMedicalRecordAsync(PatientId, new MedicalRecord { RecordId = RecordId }, CancellationToken.None));
    }

    [TestMethod]
    public async Task CreateMedicalRecordAsync_Valid_ReturnsRecordId()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns(ValidPatient());

        int result = await service.CreateMedicalRecordAsync(PatientId, new MedicalRecord { RecordId = RecordId }, CancellationToken.None);

        Assert.AreEqual(RecordId, result);
    }

    [TestMethod]
    public async Task CreatePrescriptionAsync_NoRepository_ThrowsInvalidOperation()
    {
        var patients = Substitute.For<IPatientRepository>();
        var service = new PatientService(patients);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.CreatePrescriptionAsync(RecordId, new Prescription()));
    }

    [TestMethod]
    public async Task UpdatePatientAsync_ByRequest_NotFound_ThrowsArgumentException()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetByIdAsync(PatientId).Returns((Patient?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdatePatientAsync(PatientId, new UpdatePatientRequest()));
    }

    [TestMethod]
    public async Task GetMedicalHistoryAsync_ReturnsPatientHistory()
    {
        var (service, patients, _, _) = CreateService();
        var patient = ValidPatient();
        patient.MedicalHistory = new MedicalHistory { Patient = patient, MedicalHistoryId = RecordId };
        patients.GetByIdAsync(PatientId).Returns(patient);

        var result = await service.GetMedicalHistoryAsync(PatientId);

        Assert.AreEqual(RecordId, result!.MedicalHistoryId);
    }

    [TestMethod]
    public async Task GetMedicalRecordsAsync_ReturnsRecordsForHistory()
    {
        var (service, patients, _, _) = CreateService();
        var patient = ValidPatient();
        patient.MedicalHistory = new MedicalHistory
        {
            Patient = patient,
            MedicalHistoryId = RecordId,
            MedicalRecords = [new MedicalRecord { RecordId = RecordId }],
        };
        patients.GetAllAsync().Returns(new List<Patient> { patient });

        var result = await service.GetMedicalRecordsAsync(RecordId);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task ExistsAsync_KnownCnp_ReturnsTrue()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetAllAsync().Returns(new List<Patient> { ValidPatient() });

        bool exists = await service.ExistsAsync(Cnp);

        Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task ExistsAsync_UnknownCnp_ReturnsFalse()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetAllAsync().Returns(new List<Patient>());

        bool exists = await service.ExistsAsync(Cnp);

        Assert.IsFalse(exists);
    }
}

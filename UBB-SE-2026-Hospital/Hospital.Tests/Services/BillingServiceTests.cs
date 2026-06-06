using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class BillingServiceTests
{
    private const int PercentageDivisor = 100;
    private const decimal BasePrice = 200m;
    private const int DiscountPercentage = 10;
    private const decimal ExpectedDiscountedPrice = BasePrice - BasePrice * DiscountPercentage / PercentageDivisor;

    private const decimal AppointmentBasePrice = 200m;
    private const int PatientId = 1;
    private const int RecordId = 1;

    [TestMethod]
    public async Task ApplyDiscountAsync_SubtractsPercentageFromBasePrice()
    {
        var service = new BillingService(null!, null!, null!, null!);

        decimal finalPrice = await service.ApplyDiscountAsync(BasePrice, DiscountPercentage);

        Assert.AreEqual(ExpectedDiscountedPrice, finalPrice);
    }

    [TestMethod]
    public async Task ComputeBasePriceAsync_AppointmentWithoutExtras_ReturnsAppointmentBasePrice()
    {
        var record = new MedicalRecord { SourceType = SourceType.App };
        var service = new BillingService(
            new FakeMedicalHistoryRepository(),
            new FakeMedicalRecordRepository(record),
            new FakePrescriptionRepository(),
            new FakeTransplantRepository());

        decimal basePrice = await service.ComputeBasePriceAsync(PatientId, RecordId);

        Assert.AreEqual(AppointmentBasePrice, basePrice);
    }

    private const decimal EmergencyRoomBasePrice = 500m;
    private const decimal PrescriptionItemPrice = 50m;
    private const decimal ChronicConditionPrice = 100m;
    private const decimal SevereAllergyPrice = 100m;
    private const decimal TransplantAdditionalPrice = 2000m;
    private const decimal ExpectedEmergencyPrice = EmergencyRoomBasePrice + PrescriptionItemPrice + ChronicConditionPrice + SevereAllergyPrice + TransplantAdditionalPrice;
    private const string SevereSeverity = "severe";

    [TestMethod]
    public async Task ComputeBasePriceAsync_EmergencyWithExtras_SumsAllCharges()
    {
        var records = Substitute.For<IMedicalRecordRepository>();
        var prescriptions = Substitute.For<IPrescriptionRepository>();
        var histories = Substitute.For<IMedicalHistoryRepository>();
        var transplants = Substitute.For<ITransplantRepository>();
        records.GetByIdAsync(RecordId).Returns(new MedicalRecord { SourceType = SourceType.ER, Prescription = new Prescription { PrescriptionId = 1 } });
        prescriptions.GetItemsAsync(1).Returns(new List<PrescriptionItem> { new() { MedicationName = "Aspirin" } });
        histories.GetByPatientIdAsync(PatientId).Returns(new MedicalHistory
        {
            ChronicConditions = ["Asthma"],
            PatientAllergies = [new PatientAllergy { Allergy = new Allergy { AllergyName = "Penicillin" }, SeverityLevel = SevereSeverity }],
        });
        transplants.GetByPatientIdAsync(PatientId).Returns(new List<Transplant> { new() { Receiver = new Patient() } });
        var service = new BillingService(histories, records, prescriptions, transplants);

        decimal basePrice = await service.ComputeBasePriceAsync(PatientId, RecordId);

        Assert.AreEqual(ExpectedEmergencyPrice, basePrice);
    }

    [TestMethod]
    public async Task PersistDiscountAsync_NotFound_ThrowsKeyNotFound()
    {
        var records = Substitute.For<IMedicalRecordRepository>();
        records.GetByIdAsync(RecordId).Returns((MedicalRecord?)null);
        var service = new BillingService(Substitute.For<IMedicalHistoryRepository>(), records, Substitute.For<IPrescriptionRepository>(), Substitute.For<ITransplantRepository>());

        await Assert.ThrowsExactlyAsync<KeyNotFoundException>(
            () => service.PersistDiscountAsync(RecordId, BasePrice, DiscountPercentage));
    }

    [TestMethod]
    public async Task PersistDiscountAsync_Valid_ReturnsDiscountedPrice()
    {
        var records = Substitute.For<IMedicalRecordRepository>();
        records.GetByIdAsync(RecordId).Returns(new MedicalRecord { RecordId = RecordId });
        records.UpdateAsync(Arg.Any<MedicalRecord>()).Returns(call => (MedicalRecord)call[0]);
        var service = new BillingService(Substitute.For<IMedicalHistoryRepository>(), records, Substitute.For<IPrescriptionRepository>(), Substitute.For<ITransplantRepository>());

        decimal finalPrice = await service.PersistDiscountAsync(RecordId, BasePrice, DiscountPercentage);

        Assert.AreEqual(ExpectedDiscountedPrice, finalPrice);
    }

    private sealed class FakeMedicalRecordRepository(MedicalRecord record) : IMedicalRecordRepository
    {
        public Task<MedicalRecord?> GetByIdAsync(int recordId) => Task.FromResult<MedicalRecord?>(record);
        public Task<List<MedicalRecord>> GetByMedicalHistoryIdAsync(int medicalHistoryId) => throw new NotImplementedException();
        public Task<List<MedicalRecord>> GetAllAsync() => throw new NotImplementedException();
        public Task<MedicalRecord> CreateAsync(MedicalRecord newRecord) => throw new NotImplementedException();
        public Task<MedicalRecord> UpdateAsync(MedicalRecord newRecord) => throw new NotImplementedException();
        public Task DeleteAsync(int recordId) => throw new NotImplementedException();
    }

    private sealed class FakeMedicalHistoryRepository : IMedicalHistoryRepository
    {
        public Task<MedicalHistory?> GetByIdAsync(int medicalHistoryId) => throw new NotImplementedException();
        public Task<MedicalHistory?> GetByPatientIdAsync(int patientId) => Task.FromResult<MedicalHistory?>(new MedicalHistory());
        public Task<List<MedicalHistory>> GetAllAsync() => throw new NotImplementedException();
        public Task<MedicalHistory> CreateAsync(MedicalHistory medicalHistory) => throw new NotImplementedException();
        public Task<MedicalHistory> UpdateAsync(MedicalHistory medicalHistory) => throw new NotImplementedException();
        public Task DeleteAsync(int medicalHistoryId) => throw new NotImplementedException();
    }

    private sealed class FakePrescriptionRepository : IPrescriptionRepository
    {
        public Task<Prescription?> GetByIdAsync(int prescriptionId) => throw new NotImplementedException();
        public Task<List<Prescription>> GetAllAsync() => throw new NotImplementedException();
        public Task<List<Prescription>> GetFilteredAsync(PrescriptionFilter filter) => throw new NotImplementedException();
        public Task<List<Prescription>> GetByRecordIdAsync(int recordId) => throw new NotImplementedException();
        public Task<List<Prescription>> GetPotentialDrugAddictsAsync() => throw new NotImplementedException();
        public Task<List<Prescription>> GetTopNAsync(int n, int page) => throw new NotImplementedException();
        public Task<List<PrescriptionItem>> GetItemsAsync(int prescriptionId) => Task.FromResult<List<PrescriptionItem>>([]);
        public Task MarkPoliceNotifiedAsync(int patientId) => throw new NotImplementedException();
        public Task<List<int>> GetPoliceNotifiedPatientIdsAsync(IEnumerable<int> patientIds) => throw new NotImplementedException();
        public Task<Prescription> CreateAsync(Prescription prescription) => throw new NotImplementedException();
        public Task<Prescription> UpdateAsync(Prescription prescription) => throw new NotImplementedException();
        public Task DeleteAsync(int prescriptionId) => throw new NotImplementedException();
    }

    private sealed class FakeTransplantRepository : ITransplantRepository
    {
        public Task<Transplant?> GetByIdAsync(int transplantId) => throw new NotImplementedException();
        public Task<List<Transplant>> GetAllAsync() => throw new NotImplementedException();
        public Task<List<Transplant>> GetByPatientIdAsync(int patientId) => Task.FromResult<List<Transplant>>([]);
        public Task<List<TransplantMatch>> GetMatchesForTransplantAsync(int transplantId) => throw new NotImplementedException();
        public Task<Transplant> CreateAsync(Transplant transplant) => throw new NotImplementedException();
        public Task<Transplant> UpdateAsync(Transplant transplant) => throw new NotImplementedException();
        public Task DeleteAsync(int transplantId) => throw new NotImplementedException();
        public Task<TransplantMatch> CreateMatchAsync(TransplantMatch match) => throw new NotImplementedException();
        public Task<TransplantMatch> UpdateMatchAsync(TransplantMatch match) => throw new NotImplementedException();
    }
}

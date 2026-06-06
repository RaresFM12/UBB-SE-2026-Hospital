using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class MedicalEvaluationServiceTests
{
    private const int EvaluationId = 61;
    private const int DoctorId = 2;
    private const int PatientId = 8;
    private const string Diagnosis = "Flu";
    private const string Notes = "Rest";
    private const string Medications = "Aspirin";
    private const string HighRiskMedicine = "Warfarin";
    private const string Warning = "Monitor closely.";
    private static readonly DateTime ShiftStart = DateTime.UtcNow.AddHours(-13);
    private static readonly DateTime ShiftEnd = DateTime.UtcNow;

    private static (MedicalEvaluationService Service, IEvaluationsRepository Evaluations, IHighRiskMedicineRepository HighRisk, IShiftRepository Shifts, IStaffRepository Staff) CreateService()
    {
        var evaluations = Substitute.For<IEvaluationsRepository>();
        var highRisk = Substitute.For<IHighRiskMedicineRepository>();
        var shifts = Substitute.For<IShiftRepository>();
        var staff = Substitute.For<IStaffRepository>();
        return (new MedicalEvaluationService(evaluations, highRisk, shifts, staff), evaluations, highRisk, shifts, staff);
    }

    [TestMethod]
    public async Task UpdateEvaluationAsync_NotFound_ThrowsArgumentException()
    {
        var (service, evaluations, _, _, _) = CreateService();
        evaluations.GetByIdAsync(EvaluationId).Returns((MedicalEvaluation?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateEvaluationAsync(EvaluationId, Diagnosis, Notes, Medications));
    }

    [TestMethod]
    public async Task IsDoctorFatiguedAsync_NoShifts_ReturnsFalse()
    {
        var (service, _, _, shifts, _) = CreateService();
        shifts.GetByStaffIdAsync(DoctorId).Returns(new List<Shift>());

        bool fatigued = await service.IsDoctorFatiguedAsync(DoctorId);

        Assert.IsFalse(fatigued);
    }

    [TestMethod]
    public async Task IsDoctorFatiguedAsync_LongRecentShift_ReturnsTrue()
    {
        var (service, _, _, shifts, _) = CreateService();
        shifts.GetByStaffIdAsync(DoctorId).Returns(new List<Shift> { new() { StartTime = ShiftStart, EndTime = ShiftEnd } });

        bool fatigued = await service.IsDoctorFatiguedAsync(DoctorId);

        Assert.IsTrue(fatigued);
    }

    [TestMethod]
    public async Task CheckMedicineConflictAsync_EmptyMedications_ReturnsNull()
    {
        var (service, _, _, _, _) = CreateService();

        string? conflict = await service.CheckMedicineConflictAsync(PatientId, string.Empty);

        Assert.IsNull(conflict);
    }

    [TestMethod]
    public async Task CheckMedicineConflictAsync_HighRiskMedicine_ReturnsWarning()
    {
        var (service, _, highRisk, _, _) = CreateService();
        highRisk.GetAllAsync().Returns(new List<Hospital.Data.Models.HighRiskMedicine>
        {
            new() { MedicineName = HighRiskMedicine, WarningMessage = Warning },
        });

        string? conflict = await service.CheckMedicineConflictAsync(PatientId, HighRiskMedicine);

        Assert.AreEqual(Warning, conflict);
    }

    [TestMethod]
    public async Task GetAllEvaluationsAsync_ReturnsRepositoryResult()
    {
        var (service, evaluations, _, _, _) = CreateService();
        evaluations.GetAllAsync().Returns(new List<MedicalEvaluation> { new() { EvaluationID = EvaluationId } });

        var result = await service.GetAllEvaluationsAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetEvaluationsByDoctorAsync_ReturnsRepositoryResult()
    {
        var (service, evaluations, _, _, _) = CreateService();
        evaluations.GetByDoctorIdAsync(DoctorId).Returns(new List<MedicalEvaluation> { new() { EvaluationID = EvaluationId } });

        var result = await service.GetEvaluationsByDoctorAsync(DoctorId);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetEvaluationByIdAsync_ReturnsRepositoryResult()
    {
        var (service, evaluations, _, _, _) = CreateService();
        evaluations.GetByIdAsync(EvaluationId).Returns(new MedicalEvaluation { EvaluationID = EvaluationId });

        var result = await service.GetEvaluationByIdAsync(EvaluationId);

        Assert.AreEqual(EvaluationId, result!.EvaluationID);
    }

    [TestMethod]
    public async Task CreateEvaluationAsync_PersistsEvaluation()
    {
        var (service, evaluations, _, _, _) = CreateService();

        await service.CreateEvaluationAsync(DoctorId, PatientId, Diagnosis, Notes, Medications, false);

        await evaluations.Received().CreateAsync(Arg.Any<MedicalEvaluation>());
    }

    [TestMethod]
    public async Task UpdateEvaluationAsync_Existing_PersistsChanges()
    {
        var (service, evaluations, _, _, _) = CreateService();
        evaluations.GetByIdAsync(EvaluationId).Returns(new MedicalEvaluation { EvaluationID = EvaluationId });

        await service.UpdateEvaluationAsync(EvaluationId, Diagnosis, Notes, Medications);

        await evaluations.Received().UpdateAsync(Arg.Any<MedicalEvaluation>());
    }

    [TestMethod]
    public async Task DeleteEvaluationAsync_DelegatesToRepository()
    {
        var (service, evaluations, _, _, _) = CreateService();

        await service.DeleteEvaluationAsync(EvaluationId);

        await evaluations.Received().DeleteAsync(EvaluationId);
    }

    [TestMethod]
    public async Task CheckMedicineConflictAsync_HistoricalAllergy_ReturnsAlert()
    {
        var (service, evaluations, highRisk, _, _) = CreateService();
        highRisk.GetAllAsync().Returns(new List<Hospital.Data.Models.HighRiskMedicine>());
        evaluations.GetAllAsync().Returns(new List<MedicalEvaluation>
        {
            new() { PatientId = PatientId.ToString(), Symptoms = "Allergy", MedicationsList = Medications },
        });

        string? conflict = await service.CheckMedicineConflictAsync(PatientId, Medications);

        Assert.IsNotNull(conflict);
    }

    [TestMethod]
    public async Task IsDoctorFatiguedAsync_ShortShift_ReturnsFalse()
    {
        var (service, _, _, shifts, _) = CreateService();
        shifts.GetByStaffIdAsync(DoctorId).Returns(new List<Shift> { new() { StartTime = ShiftEnd.AddHours(-2), EndTime = ShiftEnd } });

        bool fatigued = await service.IsDoctorFatiguedAsync(DoctorId);

        Assert.IsFalse(fatigued);
    }

    [TestMethod]
    public void GetAllDoctors_ReturnsRepositoryResult()
    {
        var (service, _, _, _, staff) = CreateService();
        staff.GetAllDoctorsAsync().Returns(new List<Doctor> { new() { StaffId = DoctorId } });

        var result = service.GetAllDoctors();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void GetAllEvaluations_ReturnsRepositoryResult()
    {
        var (service, evaluations, _, _, _) = CreateService();
        evaluations.GetAllAsync().Returns(new List<MedicalEvaluation> { new() { EvaluationID = EvaluationId } });

        var result = service.GetAllEvaluations();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void GetEvaluationsByDoctor_ParsesIdAndReturnsResult()
    {
        var (service, evaluations, _, _, _) = CreateService();
        evaluations.GetByDoctorIdAsync(DoctorId).Returns(new List<MedicalEvaluation> { new() { EvaluationID = EvaluationId } });

        var result = service.GetEvaluationsByDoctor(DoctorId.ToString());

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void GetEvaluationsByDoctor_NonNumericId_ReturnsEmpty()
    {
        var (service, _, _, _, _) = CreateService();

        var result = service.GetEvaluationsByDoctor("abc");

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetEvaluationById_ReturnsRepositoryResult()
    {
        var (service, evaluations, _, _, _) = CreateService();
        evaluations.GetByIdAsync(EvaluationId).Returns(new MedicalEvaluation { EvaluationID = EvaluationId });

        var result = service.GetEvaluationById(EvaluationId);

        Assert.AreEqual(EvaluationId, result!.EvaluationID);
    }

    [TestMethod]
    public void SaveEvaluation_PersistsEvaluation()
    {
        var (service, evaluations, _, _, _) = CreateService();
        var evaluation = new MedicalEvaluation { Evaluator = new Doctor { StaffId = DoctorId }, PatientId = PatientId.ToString(), Symptoms = Diagnosis, Notes = Notes, MedicationsList = Medications };

        service.SaveEvaluation(evaluation);

        evaluations.Received().CreateAsync(Arg.Any<MedicalEvaluation>());
    }

    [TestMethod]
    public void UpdateEvaluation_Existing_PersistsChanges()
    {
        var (service, evaluations, _, _, _) = CreateService();
        evaluations.GetByIdAsync(EvaluationId).Returns(new MedicalEvaluation { EvaluationID = EvaluationId });

        service.UpdateEvaluation(new MedicalEvaluation { EvaluationID = EvaluationId, Symptoms = Diagnosis, Notes = Notes, MedicationsList = Medications });

        evaluations.Received().UpdateAsync(Arg.Any<MedicalEvaluation>());
    }

    [TestMethod]
    public void DeleteEvaluation_DelegatesToRepository()
    {
        var (service, evaluations, _, _, _) = CreateService();

        service.DeleteEvaluation(EvaluationId);

        evaluations.Received().DeleteAsync(EvaluationId);
    }

    [TestMethod]
    public void IsDoctorFatigued_NonNumericId_ReturnsFalse()
    {
        var (service, _, _, _, _) = CreateService();

        bool fatigued = service.IsDoctorFatigued("abc");

        Assert.IsFalse(fatigued);
    }

    [TestMethod]
    public void CheckMedicineConflict_NonNumericId_ReturnsNull()
    {
        var (service, _, _, _, _) = CreateService();

        string? conflict = service.CheckMedicineConflict("abc", Medications);

        Assert.IsNull(conflict);
    }

    [TestMethod]
    public async Task CheckMedicineConflictAsync_NoMatch_ReturnsNull()
    {
        var (service, evaluations, highRisk, _, _) = CreateService();
        highRisk.GetAllAsync().Returns(new List<Hospital.Data.Models.HighRiskMedicine>());
        evaluations.GetAllAsync().Returns(new List<MedicalEvaluation>());

        string? conflict = await service.CheckMedicineConflictAsync(PatientId, Medications);

        Assert.IsNull(conflict);
    }
}

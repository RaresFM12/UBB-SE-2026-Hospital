using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class ExaminationServiceTests
{
    private const int ExaminationId = 14;
    private const int VisitId = 92;
    private const string Notes = "Patient stable.";

    private static (ExaminationService Service, IExaminationRepository Examinations, IERVisitRepository Visits, IERRoomRepository Rooms, ITriageRepository Triage, ITriageParametersRepository Parameters, IStaffRepository Staff) CreateService()
    {
        var examinations = Substitute.For<IExaminationRepository>();
        var visits = Substitute.For<IERVisitRepository>();
        var rooms = Substitute.For<IERRoomRepository>();
        var triage = Substitute.For<ITriageRepository>();
        var parameters = Substitute.For<ITriageParametersRepository>();
        var staff = Substitute.For<IStaffRepository>();
        return (new ExaminationService(examinations, visits, rooms, triage, parameters, staff), examinations, visits, rooms, triage, parameters, staff);
    }

    [TestMethod]
    public async Task UpdateAsync_ExaminationNotFound_ThrowsArgumentException()
    {
        var (service, examinations, _, _, _, _, _) = CreateService();
        examinations.GetByIdAsync(ExaminationId).Returns((Examination?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateAsync(new Examination { ExaminationId = ExaminationId }));
    }

    [TestMethod]
    public async Task RequestDoctorAsync_VisitNotFound_ThrowsArgumentException()
    {
        var (service, _, visits, _, _, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns((ERVisit?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.RequestDoctorAsync(VisitId));
    }

    [TestMethod]
    public async Task RequestDoctorAsync_InvalidStatus_ThrowsInvalidOperationException()
    {
        var (service, _, visits, _, _, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns(new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.REGISTERED, Patient = new Patient() });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.RequestDoctorAsync(VisitId));
    }

    [TestMethod]
    public async Task SaveExaminationAsync_VisitNotFound_ThrowsArgumentException()
    {
        var (service, _, visits, _, _, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns((ERVisit?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.SaveExaminationAsync(VisitId, Notes));
    }

    [TestMethod]
    public async Task SaveExaminationAsync_InvalidStatus_ThrowsInvalidOperationException()
    {
        var (service, _, visits, _, _, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns(new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.REGISTERED, Patient = new Patient() });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.SaveExaminationAsync(VisitId, Notes));
    }

    [TestMethod]
    public async Task SaveExaminationAsync_EmptyNotes_ThrowsArgumentException()
    {
        var (service, _, visits, _, _, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns(new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.IN_EXAMINATION, Patient = new Patient() });

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.SaveExaminationAsync(VisitId, string.Empty));
    }

    private const int TriageId = 14;
    private const int DoctorId = 6;
    private const int PatientId = 8;
    private const string DoctorSpecialization = "Neurology";

    private static ERRoom OccupiedRoom(ERVisit visit) => new()
    {
        RoomId = 101,
        AvailabilityStatus = ERRoom.RoomStatus.Occupied,
        CurrentVisit = visit,
    };

    [TestMethod]
    public async Task GetAllAsync_ReturnsRepositoryResult()
    {
        var (service, examinations, _, _, _, _, _) = CreateService();
        examinations.GetAllAsync().Returns(new List<Examination> { new() { ExaminationId = ExaminationId } });

        var result = await service.GetAllAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetByVisitIdAsync_ReturnsRepositoryResult()
    {
        var (service, examinations, _, _, _, _, _) = CreateService();
        examinations.GetByVisitIdAsync(VisitId).Returns(new List<Examination> { new() { ExaminationId = ExaminationId } });

        var result = await service.GetByVisitIdAsync(VisitId);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task UpdateAsync_Existing_PersistsFindings()
    {
        var (service, examinations, _, _, _, _, _) = CreateService();
        examinations.GetByIdAsync(ExaminationId).Returns(new Examination { ExaminationId = ExaminationId });

        await service.UpdateAsync(new Examination { ExaminationId = ExaminationId, Findings = "Stable" });

        await examinations.Received().UpdateAsync(Arg.Any<Examination>());
    }

    [TestMethod]
    public async Task GetEligibleVisitsAsync_ReturnsActiveVisits()
    {
        var (service, _, visits, rooms, _, _, _) = CreateService();
        var visit = new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.WAITING_FOR_DOCTOR, Patient = new Patient() };
        rooms.GetAllAsync().Returns(new List<ERRoom>());
        visits.GetAllAsync().Returns(new List<ERVisit> { visit });

        var result = await service.GetEligibleVisitsAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task RequestDoctorAsync_Valid_AssignsDoctor()
    {
        var (service, examinations, visits, rooms, triage, parameters, staff) = CreateService();
        var visit = new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.IN_ROOM, Patient = new Patient() };
        visits.GetByIdAsync(VisitId).Returns(visit);
        rooms.GetAllAsync().Returns(new List<ERRoom> { OccupiedRoom(visit) });
        triage.GetByVisitIdAsync(VisitId).Returns(new Triage { TriageId = TriageId, Specialization = DoctorSpecialization });
        parameters.GetByTriageIdAsync(TriageId).Returns(new TriageParameters());
        examinations.GetByVisitIdAsync(VisitId).Returns(new List<Examination>());
        staff.GetAllDoctorsAsync().Returns(new List<Doctor> { new() { StaffId = DoctorId, Specialization = DoctorSpecialization, DoctorStatus = DoctorStatus.Available } });
        examinations.CreateAsync(Arg.Any<Examination>()).Returns(call => (Examination)call[0]);

        var result = await service.RequestDoctorAsync(VisitId);

        Assert.AreEqual(DoctorId, result.Doctor.StaffId);
    }

    [TestMethod]
    public async Task SaveExaminationAsync_Valid_PersistsFindings()
    {
        var (service, examinations, visits, _, _, _, _) = CreateService();
        var visit = new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.WAITING_FOR_DOCTOR, Patient = new Patient() };
        visits.GetByIdAsync(VisitId).Returns(visit);
        examinations.GetByVisitIdAsync(VisitId).Returns(new List<Examination> { new() { ExaminationId = ExaminationId, Doctor = new Doctor { StaffId = DoctorId } } });
        examinations.UpdateAsync(Arg.Any<Examination>()).Returns(call => (Examination)call[0]);

        var result = await service.SaveExaminationAsync(VisitId, Notes);

        Assert.AreEqual(Notes, result.Findings);
    }

    [TestMethod]
    public async Task GetPatientHistoryAsync_ReturnsExaminations()
    {
        var (service, examinations, visits, _, _, _, _) = CreateService();
        visits.GetByPatientIdAsync(PatientId).Returns(new List<ERVisit> { new() { VisitId = VisitId, Patient = new Patient() } });
        examinations.GetByVisitIdAsync(VisitId).Returns(new List<Examination> { new() { ExaminationId = ExaminationId } });

        var result = await service.GetPatientHistoryAsync(PatientId);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsRepositoryResult()
    {
        var (service, examinations, _, _, _, _, _) = CreateService();
        examinations.GetByIdAsync(ExaminationId).Returns(new Examination { ExaminationId = ExaminationId });

        var result = await service.GetByIdAsync(ExaminationId);

        Assert.AreEqual(ExaminationId, result!.ExaminationId);
    }

    [TestMethod]
    public async Task CreateAsync_DelegatesToRepository()
    {
        var (service, examinations, _, _, _, _, _) = CreateService();
        var examination = new Examination { ExaminationId = ExaminationId };
        examinations.CreateAsync(examination).Returns(examination);

        var result = await service.CreateAsync(examination);

        Assert.AreEqual(ExaminationId, result.ExaminationId);
    }

    [TestMethod]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        var (service, examinations, _, _, _, _, _) = CreateService();

        await service.DeleteAsync(ExaminationId);

        await examinations.Received().DeleteAsync(ExaminationId);
    }

    [TestMethod]
    public async Task GetSummaryByVisitIdAsync_Valid_ReturnsSummary()
    {
        var (service, examinations, visits, _, triage, parameters, _) = CreateService();
        examinations.GetByVisitIdAsync(VisitId).Returns(new List<Examination>
        {
            new() { ExaminationId = ExaminationId, Findings = Notes, Doctor = new Doctor { StaffId = DoctorId } },
        });
        visits.GetByIdAsync(VisitId).Returns(new ERVisit { VisitId = VisitId, Patient = new Patient() });
        triage.GetByVisitIdAsync(VisitId).Returns(new Triage { TriageId = TriageId });
        parameters.GetByTriageIdAsync(TriageId).Returns(new TriageParameters());

        var result = await service.GetSummaryByVisitIdAsync(VisitId);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task RequestDoctorAsync_ExistingAssignment_ReturnsExisting()
    {
        var (service, examinations, visits, rooms, triage, parameters, _) = CreateService();
        var visit = new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.IN_ROOM, Patient = new Patient() };
        visits.GetByIdAsync(VisitId).Returns(visit);
        rooms.GetAllAsync().Returns(new List<ERRoom> { OccupiedRoom(visit) });
        triage.GetByVisitIdAsync(VisitId).Returns(new Triage { TriageId = TriageId, Specialization = DoctorSpecialization });
        parameters.GetByTriageIdAsync(TriageId).Returns(new TriageParameters());
        examinations.GetByVisitIdAsync(VisitId).Returns(new List<Examination> { new() { ExaminationId = ExaminationId, Doctor = new Doctor { StaffId = DoctorId } } });

        var result = await service.RequestDoctorAsync(VisitId);

        Assert.AreEqual(ExaminationId, result.ExaminationId);
    }

    [TestMethod]
    public async Task SaveExaminationAsync_DoctorInExamination_FreesDoctor()
    {
        var (service, examinations, visits, _, _, _, staff) = CreateService();
        var visit = new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.IN_EXAMINATION, Patient = new Patient() };
        visits.GetByIdAsync(VisitId).Returns(visit);
        examinations.GetByVisitIdAsync(VisitId).Returns(new List<Examination>
        {
            new() { ExaminationId = ExaminationId, Doctor = new Doctor { StaffId = DoctorId, DoctorStatus = DoctorStatus.InExamination } },
        });
        examinations.UpdateAsync(Arg.Any<Examination>()).Returns(call => (Examination)call[0]);

        await service.SaveExaminationAsync(VisitId, Notes);

        await staff.Received().UpdateAsync(Arg.Any<Staff>());
    }
}

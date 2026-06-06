using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class FatigueAuditServiceTests
{
    private const int ShiftId = 13;
    private const int StaffId = 2;
    private const int InvalidId = 0;
    private static readonly DateTime WeekStart = new(2026, 6, 1);

    private static (FatigueAuditService Service, IShiftRepository Shifts, IStaffRepository Staff) CreateService()
    {
        var shifts = Substitute.For<IShiftRepository>();
        var staff = Substitute.For<IStaffRepository>();
        return (new FatigueAuditService(shifts, staff), shifts, staff);
    }

    [TestMethod]
    public void ReassignShift_InvalidIds_ReturnsFalse()
    {
        var (service, _, _) = CreateService();

        bool reassigned = service.ReassignShift(InvalidId, StaffId);

        Assert.IsFalse(reassigned);
    }

    [TestMethod]
    public void ReassignShift_ShiftNotFound_ReturnsFalse()
    {
        var (service, shifts, staff) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns((Shift?)null);
        staff.GetByIdAsync(StaffId).Returns(new Staff { StaffId = StaffId });

        bool reassigned = service.ReassignShift(ShiftId, StaffId);

        Assert.IsFalse(reassigned);
    }

    [TestMethod]
    public void ReassignShift_ValidShiftAndStaff_ReturnsTrue()
    {
        var (service, shifts, staff) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns(new Shift { Id = ShiftId, Staff = new Staff { StaffId = StaffId + 1 } });
        staff.GetByIdAsync(StaffId).Returns(new Staff { StaffId = StaffId });

        bool reassigned = service.ReassignShift(ShiftId, StaffId);

        Assert.IsTrue(reassigned);
    }

    [TestMethod]
    public void RunAutoAudit_NoStaffNoShifts_ReportsNoConflicts()
    {
        var (service, shifts, staff) = CreateService();
        staff.GetAllAsync().Returns(new List<Staff>());
        shifts.GetAllAsync().Returns(new List<Shift>());

        var result = service.RunAutoAudit(WeekStart);

        Assert.IsFalse(result.HasConflicts);
    }

    private const int OverworkedDoctorId = 1;
    private const int ReplacementDoctorId = 2;
    private const string AuditSpecialization = "Cardiology";
    private const double LongShiftHours = 35.0;
    private static readonly DateTime MondayWeekStart = new(2026, 6, 1);

    [TestMethod]
    public void RunAutoAudit_WeeklyHoursExceeded_ReportsConflict()
    {
        var (service, shifts, staff) = CreateService();
        staff.GetAllAsync().Returns(new List<Staff>
        {
            new Doctor { StaffId = OverworkedDoctorId, Specialization = AuditSpecialization, Available = true },
            new Doctor { StaffId = ReplacementDoctorId, Specialization = AuditSpecialization, Available = true },
        });
        shifts.GetAllAsync().Returns(new List<Shift>
        {
            new() { Id = 1, Staff = new Staff { StaffId = OverworkedDoctorId }, StartTime = MondayWeekStart, EndTime = MondayWeekStart.AddHours(LongShiftHours), Status = ShiftStatus.Scheduled },
            new() { Id = 2, Staff = new Staff { StaffId = OverworkedDoctorId }, StartTime = MondayWeekStart.AddDays(2), EndTime = MondayWeekStart.AddDays(2).AddHours(LongShiftHours), Status = ShiftStatus.Scheduled },
        });

        var result = service.RunAutoAudit(MondayWeekStart);

        Assert.IsTrue(result.HasConflicts);
    }

    [TestMethod]
    public void RunAutoAudit_WeeklyHoursExceeded_SuggestsReplacement()
    {
        var (service, shifts, staff) = CreateService();
        staff.GetAllAsync().Returns(new List<Staff>
        {
            new Doctor { StaffId = OverworkedDoctorId, Specialization = AuditSpecialization, Available = true },
            new Doctor { StaffId = ReplacementDoctorId, Specialization = AuditSpecialization, Available = true },
        });
        shifts.GetAllAsync().Returns(new List<Shift>
        {
            new() { Id = 1, Staff = new Staff { StaffId = OverworkedDoctorId }, StartTime = MondayWeekStart, EndTime = MondayWeekStart.AddHours(LongShiftHours), Status = ShiftStatus.Scheduled },
            new() { Id = 2, Staff = new Staff { StaffId = OverworkedDoctorId }, StartTime = MondayWeekStart.AddDays(2), EndTime = MondayWeekStart.AddDays(2).AddHours(LongShiftHours), Status = ShiftStatus.Scheduled },
        });

        var result = service.RunAutoAudit(MondayWeekStart);

        Assert.AreEqual(ReplacementDoctorId, result.Suggestions[0].SuggestedStaffId);
    }

    [TestMethod]
    public void RunAutoAudit_InsufficientRestBetweenShifts_ReportsConflict()
    {
        var (service, shifts, staff) = CreateService();
        staff.GetAllAsync().Returns(new List<Staff>
        {
            new Doctor { StaffId = OverworkedDoctorId, Specialization = AuditSpecialization, Available = true },
        });
        shifts.GetAllAsync().Returns(new List<Shift>
        {
            new() { Id = 1, Staff = new Staff { StaffId = OverworkedDoctorId }, StartTime = MondayWeekStart, EndTime = MondayWeekStart.AddHours(8), Status = ShiftStatus.Scheduled },
            new() { Id = 2, Staff = new Staff { StaffId = OverworkedDoctorId }, StartTime = MondayWeekStart.AddHours(10), EndTime = MondayWeekStart.AddHours(18), Status = ShiftStatus.Scheduled },
        });

        var result = service.RunAutoAudit(MondayWeekStart);

        Assert.IsTrue(result.HasConflicts);
    }

    [TestMethod]
    public void ReassignShift_InvalidNewStaffId_ReturnsFalse()
    {
        var (service, _, _) = CreateService();

        bool reassigned = service.ReassignShift(ShiftId, InvalidId);

        Assert.IsFalse(reassigned);
    }
}

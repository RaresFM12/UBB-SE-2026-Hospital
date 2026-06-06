using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using Hospital.Shared.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class TriageServiceTests
{
    private const int VisitId = 91;
    private const int TriageId = 14;

    private static (TriageService Service, ITriageRepository Triage, IERVisitRepository Visits, ITriageParametersRepository Parameters, ITriageDecisionService Decision) CreateService()
    {
        var triage = Substitute.For<ITriageRepository>();
        var visits = Substitute.For<IERVisitRepository>();
        var parameters = Substitute.For<ITriageParametersRepository>();
        var decision = Substitute.For<ITriageDecisionService>();
        return (new TriageService(triage, visits, parameters, decision), triage, visits, parameters, decision);
    }

    [TestMethod]
    public async Task CreateTriageAsync_VisitNotFound_ThrowsArgumentException()
    {
        var (service, _, visits, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns((ERVisit?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateTriageAsync(VisitId, new PerformTriageRequest()));
    }

    [TestMethod]
    public async Task CreateTriageAsync_VisitNotRegistered_ThrowsInvalidOperationException()
    {
        var (service, _, visits, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns(new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.TRIAGED, Patient = new Patient() });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.CreateTriageAsync(VisitId, new PerformTriageRequest()));
    }

    [TestMethod]
    public async Task UpdateAsync_TriageNotFound_ThrowsArgumentException()
    {
        var (service, triage, _, _, _) = CreateService();
        triage.GetByIdAsync(TriageId).Returns((Triage?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateAsync(new Triage { TriageId = TriageId }));
    }

    [TestMethod]
    public async Task MoveVisitToQueueAsync_VisitNotFound_ThrowsArgumentException()
    {
        var (service, _, visits, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns((ERVisit?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.MoveVisitToQueueAsync(VisitId));
    }

    [TestMethod]
    public async Task MoveVisitToQueueAsync_VisitNotTriaged_ThrowsInvalidOperationException()
    {
        var (service, _, visits, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns(new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.REGISTERED, Patient = new Patient() });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.MoveVisitToQueueAsync(VisitId));
    }

    [TestMethod]
    public async Task CloseVisitAsync_VisitNotFound_ThrowsArgumentException()
    {
        var (service, _, visits, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns((ERVisit?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CloseVisitAsync(VisitId));
    }

    [TestMethod]
    public async Task GetVisitsForTriageAsync_ReturnsOnlyRegisteredVisits()
    {
        var (service, _, visits, _, _) = CreateService();
        visits.GetAllAsync().Returns(new List<ERVisit>
        {
            new() { VisitId = VisitId, Status = ERVisit.VisitStatus.REGISTERED, Patient = new Patient() },
            new() { VisitId = VisitId + 1, Status = ERVisit.VisitStatus.CLOSED, Patient = new Patient() },
        });

        var result = await service.GetVisitsForTriageAsync();

        Assert.HasCount(1, result);
    }

    private const string Specialization = "Cardiology";
    private const int ValidTriageLevel = 2;

    private static PerformTriageRequest ValidRequest() => new()
    {
        NurseId = 1,
        TriageLevel = ValidTriageLevel,
        Specialization = Specialization,
        Consciousness = 1,
        Breathing = 1,
        Bleeding = 1,
        InjuryType = 1,
        PainLevel = 1,
    };

    [TestMethod]
    public async Task GetAllAsync_ReturnsRepositoryResult()
    {
        var (service, triage, _, _, _) = CreateService();
        triage.GetAllAsync().Returns(new List<Triage> { new() { TriageId = TriageId } });

        var result = await service.GetAllAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetByVisitIdAsync_ReturnsRepositoryResult()
    {
        var (service, triage, _, _, _) = CreateService();
        triage.GetByVisitIdAsync(VisitId).Returns(new Triage { TriageId = TriageId });

        var result = await service.GetByVisitIdAsync(VisitId);

        Assert.AreEqual(TriageId, result!.TriageId);
    }

    [TestMethod]
    public async Task UpdateAsync_Existing_PersistsTriage()
    {
        var (service, triage, _, _, _) = CreateService();
        triage.GetByIdAsync(TriageId).Returns(new Triage { TriageId = TriageId });

        await service.UpdateAsync(new Triage { TriageId = TriageId, Specialization = Specialization });

        await triage.Received().UpdateAsync(Arg.Any<Triage>());
    }

    [TestMethod]
    public async Task CreateTriageAsync_Valid_CreatesTriage()
    {
        var (service, triage, visits, parameters, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns(new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.REGISTERED, Patient = new Patient() });
        triage.GetByVisitIdAsync(VisitId).Returns((Triage?)null);
        triage.CreateAsync(Arg.Any<Triage>()).Returns(call => (Triage)call[0]);

        var result = await service.CreateTriageAsync(VisitId, ValidRequest());

        Assert.AreEqual(ValidTriageLevel, result.TriageLevel);
    }

    [TestMethod]
    public async Task MoveVisitToQueueAsync_Valid_SetsWaitingForRoom()
    {
        var (service, triage, visits, parameters, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns(new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.TRIAGED, Patient = new Patient() });
        triage.GetByVisitIdAsync(VisitId).Returns(new Triage { TriageId = TriageId });
        parameters.GetByTriageIdAsync(TriageId).Returns(new TriageParameters());

        await service.MoveVisitToQueueAsync(VisitId);

        await visits.Received().UpdateAsync(Arg.Is<ERVisit>(visit => visit.Status == ERVisit.VisitStatus.WAITING_FOR_ROOM));
    }

    [TestMethod]
    public async Task CloseVisitAsync_Valid_SetsClosedStatus()
    {
        var (service, _, visits, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns(new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.IN_EXAMINATION, Patient = new Patient() });

        await service.CloseVisitAsync(VisitId);

        await visits.Received().UpdateAsync(Arg.Is<ERVisit>(visit => visit.Status == ERVisit.VisitStatus.CLOSED));
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsRepositoryResult()
    {
        var (service, triage, _, _, _) = CreateService();
        triage.GetByIdAsync(TriageId).Returns(new Triage { TriageId = TriageId });

        var result = await service.GetByIdAsync(TriageId);

        Assert.AreEqual(TriageId, result!.TriageId);
    }

    [TestMethod]
    public async Task CreateAsync_DelegatesToRepository()
    {
        var (service, triage, _, _, _) = CreateService();
        var entity = new Triage { TriageId = TriageId };
        triage.CreateAsync(entity).Returns(entity);

        var result = await service.CreateAsync(entity);

        Assert.AreEqual(TriageId, result.TriageId);
    }

    [TestMethod]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        var (service, triage, _, _, _) = CreateService();

        await service.DeleteAsync(TriageId);

        await triage.Received().DeleteAsync(TriageId);
    }

    [TestMethod]
    public async Task CreateTriageAsync_ExistingTriage_IsReplaced()
    {
        var (service, triage, visits, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns(new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.REGISTERED, Patient = new Patient() });
        triage.GetByVisitIdAsync(VisitId).Returns(new Triage { TriageId = TriageId });
        triage.CreateAsync(Arg.Any<Triage>()).Returns(call => (Triage)call[0]);

        await service.CreateTriageAsync(VisitId, ValidRequest());

        await triage.Received().DeleteAsync(TriageId);
    }
}

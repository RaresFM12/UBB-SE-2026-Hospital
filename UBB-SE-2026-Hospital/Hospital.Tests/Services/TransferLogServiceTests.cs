using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class TransferLogServiceTests
{
    private const int EligibleVisitId = 1;
    private const int IgnoredVisitId = 2;

    private static (TransferLogService Service, ITransferLogRepository Logs, IERVisitRepository Visits) CreateService()
    {
        var logs = Substitute.For<ITransferLogRepository>();
        var visits = Substitute.For<IERVisitRepository>();
        return (new TransferLogService(logs, visits), logs, visits);
    }

    [TestMethod]
    public async Task GetEligibleVisitsAsync_IncludesOnlyExaminationOrTransferredVisits()
    {
        var (service, _, visits) = CreateService();
        visits.GetAllAsync().Returns(new List<ERVisit>
        {
            new() { VisitId = EligibleVisitId, Status = ERVisit.VisitStatus.IN_EXAMINATION, Patient = new Patient() },
            new() { VisitId = IgnoredVisitId, Status = ERVisit.VisitStatus.REGISTERED, Patient = new Patient() },
        });

        var result = await service.GetEligibleVisitsAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsRepositoryResult()
    {
        var (service, logs, _) = CreateService();
        logs.GetAllAsync().Returns(new List<TransferLog> { new() });

        var result = await service.GetAllAsync();

        Assert.HasCount(1, result);
    }

    private const int TransferLogId = 31;

    [TestMethod]
    public async Task GetByIdAsync_ReturnsRepositoryResult()
    {
        var (service, logs, _) = CreateService();
        logs.GetByIdAsync(TransferLogId).Returns(new TransferLog { TransferLogId = TransferLogId });

        var result = await service.GetByIdAsync(TransferLogId);

        Assert.AreEqual(TransferLogId, result!.TransferLogId);
    }

    [TestMethod]
    public async Task CreateAsync_DelegatesToRepository()
    {
        var (service, logs, _) = CreateService();
        var log = new TransferLog { TransferLogId = TransferLogId };
        logs.CreateAsync(log).Returns(log);

        var result = await service.CreateAsync(log);

        Assert.AreEqual(TransferLogId, result.TransferLogId);
    }

    [TestMethod]
    public async Task UpdateAsync_DelegatesToRepository()
    {
        var (service, logs, _) = CreateService();

        await service.UpdateAsync(new TransferLog { TransferLogId = TransferLogId });

        await logs.Received().UpdateAsync(Arg.Any<TransferLog>());
    }

    [TestMethod]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        var (service, logs, _) = CreateService();

        await service.DeleteAsync(TransferLogId);

        await logs.Received().DeleteAsync(TransferLogId);
    }

    [TestMethod]
    public async Task GetByVisitIdAsync_ReturnsRepositoryResult()
    {
        var (service, logs, _) = CreateService();
        logs.GetByVisitIdAsync(EligibleVisitId).Returns(new List<TransferLog> { new() { TransferLogId = TransferLogId } });

        var result = await service.GetByVisitIdAsync(EligibleVisitId);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetEligibleVisitsAsync_TransferredVisit_IsIncluded()
    {
        var (service, _, visits) = CreateService();
        visits.GetAllAsync().Returns(new List<ERVisit>
        {
            new() { VisitId = EligibleVisitId, Status = ERVisit.VisitStatus.TRANSFERRED, Patient = new Patient { Transferred = true } },
        });

        var result = await service.GetEligibleVisitsAsync();

        Assert.IsTrue(result[0].Transferred);
    }
}

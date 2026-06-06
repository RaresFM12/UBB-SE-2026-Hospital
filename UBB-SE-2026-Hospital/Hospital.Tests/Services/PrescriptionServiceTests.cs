using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class PrescriptionServiceTests
{
    private const int PrescriptionId = 81;
    private const int PageSize = 20;
    private const int PageNumber = 1;

    [TestMethod]
    public async Task GetPrescriptionDetailsAsync_NotFound_ThrowsArgumentException()
    {
        var repository = Substitute.For<IPrescriptionRepository>();
        repository.GetFilteredAsync(Arg.Any<PrescriptionFilter>()).Returns(new List<Prescription>());
        var service = new PrescriptionService(repository);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.GetPrescriptionDetailsAsync(PrescriptionId));
    }

    [TestMethod]
    public async Task GetPrescriptionDetailsAsync_Found_ReturnsPrescription()
    {
        var repository = Substitute.For<IPrescriptionRepository>();
        repository.GetFilteredAsync(Arg.Any<PrescriptionFilter>())
            .Returns(new List<Prescription> { new() { PrescriptionId = PrescriptionId } });
        var service = new PrescriptionService(repository);

        var prescription = await service.GetPrescriptionDetailsAsync(PrescriptionId);

        Assert.AreEqual(PrescriptionId, prescription.PrescriptionId);
    }

    [TestMethod]
    public async Task ApplyFilterAsync_NullFilter_FallsBackToTopN()
    {
        var repository = Substitute.For<IPrescriptionRepository>();
        repository.GetTopNAsync(PageSize, PageNumber).Returns(new List<Prescription>());
        var service = new PrescriptionService(repository);

        await service.ApplyFilterAsync(null!);

        await repository.Received().GetTopNAsync(PageSize, PageNumber);
    }

    [TestMethod]
    public async Task GetLatestPrescriptionsAsync_DelegatesToRepository()
    {
        var repository = Substitute.For<IPrescriptionRepository>();
        repository.GetTopNAsync(PageSize, PageNumber).Returns(new List<Prescription> { new() });
        var service = new PrescriptionService(repository);

        var result = await service.GetLatestPrescriptionsAsync(PageSize, PageNumber);

        Assert.HasCount(1, result);
    }
}

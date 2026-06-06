using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class PharmacyHandoverServiceTests
{
    private const int ExpectedCount = 1;

    [TestMethod]
    public async Task GetAllPharmacyHandoversAsync_ReturnsRepositoryResult()
    {
        var repository = Substitute.For<IPharmacyHandoverRepository>();
        repository.GetAllAsync().Returns(new List<PharmacyHandover> { new() });
        var service = new PharmacyHandoverService(repository);

        var result = await service.GetAllPharmacyHandoversAsync();

        Assert.HasCount(ExpectedCount, result);
    }
}

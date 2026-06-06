using Hospital.Data.Repositories;
using Hospital.Services;
using Hospital.Shared.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class PeriodTrackerServiceFactoryTests
{
    private static PeriodTrackerServiceFactory CreateFactory()
    {
        var users = Substitute.For<IUsersRepository>();
        var items = Substitute.For<IItemsRepository>();
        var currentUser = Substitute.For<ICurrentUserService>();
        var orderService = Substitute.For<IOrderService>();
        return new PeriodTrackerServiceFactory(users, items, currentUser, orderService);
    }

    [TestMethod]
    public void CreatePeriodTrackerService_ReturnsInstance()
    {
        var factory = CreateFactory();

        var service = factory.CreatePeriodTrackerService();

        Assert.IsInstanceOfType<PeriodTrackerService>(service);
    }

    [TestMethod]
    public void CreateWellnessItemsService_ReturnsInstance()
    {
        var factory = CreateFactory();

        var service = factory.CreateWellnessItemsService();

        Assert.IsInstanceOfType<WellnessItemsService>(service);
    }

    [TestMethod]
    public void CreateBasketService_ThrowsNotImplemented()
    {
        var factory = CreateFactory();

        Assert.ThrowsExactly<NotImplementedException>(() => factory.CreateBasketService());
    }
}

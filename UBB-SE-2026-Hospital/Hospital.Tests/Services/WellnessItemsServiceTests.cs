using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class WellnessItemsServiceTests
{
    private const string WellnessCategory = "wellness";
    private const string OtherCategory = "antibiotics";

    [TestMethod]
    public void GetWellnessItems_ReturnsOnlyWellnessCategory()
    {
        var repository = Substitute.For<IItemsRepository>();
        repository.GetAllAsync().Returns(new List<Item>
        {
            new() { Id = 1, Category = WellnessCategory },
            new() { Id = 2, Category = OtherCategory },
        });
        var service = new WellnessItemsService(repository);

        var result = service.GetWellnessItems();

        Assert.HasCount(1, result);
    }
}

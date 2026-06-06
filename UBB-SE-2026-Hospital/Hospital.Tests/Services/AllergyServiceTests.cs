using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class AllergyServiceTests
{
    private const string AllergyName = "Penicillin";

    [TestMethod]
    public async Task GetAllergiesAsync_ReturnsRepositoryResult()
    {
        var repository = Substitute.For<IAllergyRepository>();
        repository.GetAllAsync().Returns(new List<Allergy> { new() { AllergyName = AllergyName } });
        var service = new AllergyService(repository);

        var allergies = await service.GetAllergiesAsync();

        Assert.AreEqual(AllergyName, allergies[0].AllergyName);
    }
}

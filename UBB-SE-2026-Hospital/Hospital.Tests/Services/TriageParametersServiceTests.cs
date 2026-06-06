using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class TriageParametersServiceTests
{
    private const int ParametersId = 71;
    private const int ValidLevel = 2;
    private const int InvalidLevel = 9;

    private static TriageParameters Valid(int parametersId = 0)
        => new()
        {
            TriageParametersId = parametersId,
            Consciousness = ValidLevel,
            Breathing = ValidLevel,
            Bleeding = ValidLevel,
            InjuryType = ValidLevel,
            PainLevel = ValidLevel,
        };

    [TestMethod]
    public async Task CreateAsync_InvalidParameters_ThrowsArgumentOutOfRange()
    {
        var repository = Substitute.For<ITriageParametersRepository>();
        var service = new TriageParametersService(repository);
        var parameters = Valid();
        parameters.Consciousness = InvalidLevel;

        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(
            () => service.CreateAsync(parameters));
    }

    [TestMethod]
    public async Task UpdateAsync_NotFound_ThrowsArgumentException()
    {
        var repository = Substitute.For<ITriageParametersRepository>();
        repository.GetByIdAsync(ParametersId).Returns((TriageParameters?)null);
        var service = new TriageParametersService(repository);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateAsync(Valid(ParametersId)));
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsRepositoryResult()
    {
        var repository = Substitute.For<ITriageParametersRepository>();
        repository.GetAllAsync().Returns(new List<TriageParameters> { Valid() });
        var service = new TriageParametersService(repository);

        var result = await service.GetAllAsync();

        Assert.HasCount(1, result);
    }

    private const int TriageId = 14;

    [TestMethod]
    public async Task GetByIdAsync_ReturnsRepositoryResult()
    {
        var repository = Substitute.For<ITriageParametersRepository>();
        repository.GetByIdAsync(ParametersId).Returns(Valid(ParametersId));
        var service = new TriageParametersService(repository);

        var result = await service.GetByIdAsync(ParametersId);

        Assert.AreEqual(ParametersId, result!.TriageParametersId);
    }

    [TestMethod]
    public async Task GetByTriageIdAsync_ReturnsRepositoryResult()
    {
        var repository = Substitute.For<ITriageParametersRepository>();
        repository.GetByTriageIdAsync(TriageId).Returns(Valid());
        var service = new TriageParametersService(repository);

        var result = await service.GetByTriageIdAsync(TriageId);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task CreateAsync_Valid_CreatesParameters()
    {
        var repository = Substitute.For<ITriageParametersRepository>();
        repository.CreateAsync(Arg.Any<TriageParameters>()).Returns(call => (TriageParameters)call[0]);
        var service = new TriageParametersService(repository);

        var result = await service.CreateAsync(Valid());

        Assert.AreEqual(ValidLevel, result.Consciousness);
    }

    [TestMethod]
    public async Task UpdateAsync_Existing_PersistsChanges()
    {
        var repository = Substitute.For<ITriageParametersRepository>();
        repository.GetByIdAsync(ParametersId).Returns(Valid(ParametersId));
        repository.UpdateAsync(Arg.Any<TriageParameters>()).Returns(call => (TriageParameters)call[0]);
        var service = new TriageParametersService(repository);

        var result = await service.UpdateAsync(Valid(ParametersId));

        Assert.AreEqual(ParametersId, result.TriageParametersId);
    }

    [TestMethod]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        var repository = Substitute.For<ITriageParametersRepository>();
        var service = new TriageParametersService(repository);

        await service.DeleteAsync(ParametersId);

        await repository.Received().DeleteAsync(ParametersId);
    }
}

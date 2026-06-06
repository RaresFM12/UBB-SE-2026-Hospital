using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class SubstancesEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAll_AsPharmacist_ReturnsSeededReferenceData()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var substances = await client.GetFromJsonAsync<List<Substance>>("/api/substances", JsonOptions);

        Assert.IsNotNull(substances);
        // 19 substances are seeded via HasData in the DbContext.
        Assert.IsTrue(substances!.Count >= 19);
        Assert.IsTrue(substances.Any(substance => substance.Name == "Ibuprofen"));
    }

    [TestMethod]
    public async Task GetAll_AsDoctor_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/substances");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetByName_Existing_ReturnsSubstance()
    {
        using var client = await CreateAdminClientAsync();

        var substance = await client.GetFromJsonAsync<Substance>("/api/substances/Paracetamol", JsonOptions);

        Assert.IsNotNull(substance);
        Assert.AreEqual("Paracetamol", substance!.Name);
    }

    [TestMethod]
    public async Task GetByName_Missing_ReturnsNotFound()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/substances/Unobtainium");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Exists_ReturnsTrueForSeeded_AndFalseForUnknown()
    {
        using var client = await CreateAdminClientAsync();

        var existing = await client.GetFromJsonAsync<bool>("/api/substances/Iron/exists", JsonOptions);
        var missing = await client.GetFromJsonAsync<bool>("/api/substances/Kryptonite/exists", JsonOptions);

        Assert.IsTrue(existing);
        Assert.IsFalse(missing);
    }

    [TestMethod]
    public async Task GetTop_ReturnsDictionary()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/substances/top");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var top = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>(JsonOptions);
        Assert.IsNotNull(top);
    }

    [TestMethod]
    public async Task Create_NewSubstance_PersistsIt()
    {
        using var client = await CreateAdminClientAsync();
        var request = new { Name = "Testium", LethalDose = 500.0f, Description = "Synthetic test compound" };

        var response = await client.PostAsJsonAsync("/api/substances", request);

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        var created = await client.GetFromJsonAsync<Substance>("/api/substances/Testium", JsonOptions);
        Assert.AreEqual(500.0f, created!.LethalDose);
    }

    [TestMethod]
    public async Task Create_DuplicateSubstance_ThrowsArgumentException()
    {
        using var client = await CreateAdminClientAsync();
        var request = new { Name = "Magnesium", LethalDose = 1.0f, Description = "duplicate" };

        // The substance already exists (seeded), so the service rejects it with an
        // ArgumentException. These controllers have no exception handler, so the
        // test host surfaces the domain exception directly to the caller.
        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => client.PostAsJsonAsync("/api/substances", request));
    }

    [TestMethod]
    public async Task Update_NonExistentSubstance_ThrowsArgumentException()
    {
        using var client = await CreateAdminClientAsync();
        var ghost = new Substance { Name = "Ghostium", LethalDose = 1.0f, Description = "missing" };

        // The controller binds the name from the route; updating an unknown
        // substance hits the validation guard which throws ArgumentException.
        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => client.PutAsJsonAsync("/api/substances/Ghostium", ghost));
    }

    [TestMethod]
    public async Task Delete_ExistingSubstance_RemovesIt()
    {
        using var client = await CreateAdminClientAsync();
        await client.PostAsJsonAsync("/api/substances",
            new { Name = "Deletium", LethalDose = 10.0f, Description = "to be removed" });

        var response = await client.DeleteAsync("/api/substances/Deletium");

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        var exists = await client.GetFromJsonAsync<bool>("/api/substances/Deletium/exists", JsonOptions);
        Assert.IsFalse(exists);
    }
}

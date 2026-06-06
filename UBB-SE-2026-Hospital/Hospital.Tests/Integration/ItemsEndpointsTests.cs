using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class ItemsEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAll_AsPharmacist_ReturnsSeededItem()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var items = await client.GetFromJsonAsync<List<Item>>("/api/items", JsonOptions);

        Assert.IsNotNull(items);
        Assert.IsTrue(items!.Any(i => i.Name == "Aspirin"));
    }

    [TestMethod]
    public async Task GetAll_WithNameFilter_ReturnsMatchingItems()
    {
        using var client = await CreateAdminClientAsync();

        var items = await client.GetFromJsonAsync<List<Item>>("/api/items?name=Aspirin", JsonOptions);

        Assert.IsNotNull(items);
        Assert.IsTrue(items!.All(i => i.Name.Contains("Aspirin", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public async Task GetAll_AsDoctor_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/items");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetById_Existing_ReturnsItem()
    {
        using var client = await CreateAdminClientAsync();

        var item = await client.GetFromJsonAsync<Item>($"/api/items/{Ids.ItemId}", JsonOptions);

        Assert.IsNotNull(item);
        Assert.AreEqual("Aspirin", item!.Name);
    }

    [TestMethod]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/items/987654");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Exists_ReturnsTrueForSeeded_AndFalseForUnknown()
    {
        using var client = await CreateAdminClientAsync();

        var existing = await client.GetFromJsonAsync<bool>($"/api/items/{Ids.ItemId}/exists", JsonOptions);
        var missing = await client.GetFromJsonAsync<bool>("/api/items/987654/exists", JsonOptions);

        Assert.IsTrue(existing);
        Assert.IsFalse(missing);
    }

    [TestMethod]
    public async Task GetExpired_ReturnsOk()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/items/expired");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetTop_ReturnsOk()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/items/top");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_NewItem_PersistsAndIsListable()
    {
        using var client = await CreateAdminClientAsync();
        var request = new
        {
            Name = "Paracetamol Forte",
            Producer = "Zentiva",
            Category = "Painkiller",
            Price = 9.9f,
            NumberOfPills = 10,
            Label = "OTC",
            Description = "Fever reducer",
            ImagePath = "img.png",
            Discount = 0f,
        };

        var response = await client.PostAsJsonAsync("/api/items", request);

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        var items = await client.GetFromJsonAsync<List<Item>>("/api/items?name=Paracetamol Forte", JsonOptions);
        Assert.IsTrue(items!.Any(i => i.Name == "Paracetamol Forte"));
    }

    [TestMethod]
    public async Task Create_InvalidItem_ThrowsArgumentException()
    {
        using var client = await CreateAdminClientAsync();
        var request = new
        {
            Name = "",
            Producer = "",
            Category = "X",
            Price = 0f,
            NumberOfPills = 0,
            Label = "",
            Description = "",
            ImagePath = "",
            Discount = 0f,
        };

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => client.PostAsJsonAsync("/api/items", request));
    }

    [TestMethod]
    public async Task Delete_ExistingItem_RemovesIt()
    {
        using var client = await CreateAdminClientAsync();
        await client.PostAsJsonAsync("/api/items", new
        {
            Name = "Temp Item",
            Producer = "ACME",
            Category = "Misc",
            Price = 5f,
            NumberOfPills = 5,
            Label = "L",
            Description = "D",
            ImagePath = "i.png",
            Discount = 0f,
        });
        var created = (await client.GetFromJsonAsync<List<Item>>("/api/items?name=Temp Item", JsonOptions))!.Single();

        var response = await client.DeleteAsync($"/api/items/{created.Id}");

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        var exists = await client.GetFromJsonAsync<bool>($"/api/items/{created.Id}/exists", JsonOptions);
        Assert.IsFalse(exists);
    }
}

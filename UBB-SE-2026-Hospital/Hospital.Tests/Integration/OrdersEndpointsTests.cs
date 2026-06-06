using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class OrdersEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAll_AsPharmacist_ReturnsSeededOrder()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var orders = await client.GetFromJsonAsync<List<Order>>("/api/orders", JsonOptions);

        Assert.IsNotNull(orders);
        Assert.IsTrue(orders!.Any(order => order.Id == Ids.OrderId));
    }

    [TestMethod]
    public async Task GetAll_FilteredByClient_ReturnsOnlyClientOrders()
    {
        using var client = await CreateAdminClientAsync();

        var orders = await client.GetFromJsonAsync<List<Order>>(
            $"/api/orders?clientId={Ids.ClientUserId}", JsonOptions);

        Assert.IsNotNull(orders);
        Assert.IsTrue(orders!.All(order => order.ClientId == Ids.ClientUserId));
    }

    [TestMethod]
    public async Task GetAll_AsDoctor_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/orders");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetById_Existing_ReturnsOrder()
    {
        using var client = await CreateAdminClientAsync();

        var order = await client.GetFromJsonAsync<Order>($"/api/orders/{Ids.OrderId}", JsonOptions);

        Assert.IsNotNull(order);
        Assert.AreEqual(Ids.OrderId, order!.Id);
    }

    [TestMethod]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/orders/555555");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Exists_ReturnsTrueForSeeded_AndFalseForUnknown()
    {
        using var client = await CreateAdminClientAsync();

        var existing = await client.GetFromJsonAsync<bool>($"/api/orders/{Ids.OrderId}/exists", JsonOptions);
        var missing = await client.GetFromJsonAsync<bool>("/api/orders/555555/exists", JsonOptions);

        Assert.IsTrue(existing);
        Assert.IsFalse(missing);
    }

    [TestMethod]
    public async Task Create_NewOrder_ReturnsGeneratedId()
    {
        using var client = await CreateAdminClientAsync();
        var request = new
        {
            ClientId = Ids.ClientUserId,
            PickUpDate = new DateOnly(2025, 3, 1),
            IsCompleted = false,
            IsExpired = false,
        };

        var response = await client.PostAsJsonAsync("/api/orders", request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var newId = await response.Content.ReadFromJsonAsync<int>(JsonOptions);
        Assert.IsTrue(newId > 0);
    }

    [TestMethod]
    public async Task Create_UnknownClient_ThrowsArgumentException()
    {
        using var client = await CreateAdminClientAsync();
        var request = new
        {
            ClientId = 999999,
            PickUpDate = new DateOnly(2025, 3, 1),
            IsCompleted = false,
            IsExpired = false,
        };

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => client.PostAsJsonAsync("/api/orders", request));
    }

    [TestMethod]
    public async Task Delete_ExistingOrder_RemovesIt()
    {
        using var client = await CreateAdminClientAsync();
        var createResponse = await client.PostAsJsonAsync("/api/orders", new
        {
            ClientId = Ids.ClientUserId,
            PickUpDate = new DateOnly(2025, 4, 1),
            IsCompleted = false,
            IsExpired = false,
        });
        var newId = await createResponse.Content.ReadFromJsonAsync<int>(JsonOptions);

        var response = await client.DeleteAsync($"/api/orders/{newId}");

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        var exists = await client.GetFromJsonAsync<bool>($"/api/orders/{newId}/exists", JsonOptions);
        Assert.IsFalse(exists);
    }
}

using System.Net;
using System.Net.Http.Json;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class TriageEndpointsTests : IntegrationTestBase
{
    private sealed record TriageDecisionResponse(int TriageLevel, string Specialization);

    [TestMethod]
    public async Task GetAll_AsDoctor_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/triage");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_AsNurse_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.NurseEmail);

        var response = await client.GetAsync("/api/triage");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_AsPharmacist_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var response = await client.GetAsync("/api/triage");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/triage");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/triage/999999");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Decide_WithValidParameters_ReturnsDecision()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.PostAsJsonAsync("/api/triage/decide", BuildParameters(1, 1, 1, 1, 1));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var decision = await response.Content.ReadFromJsonAsync<TriageDecisionResponse>(JsonOptions);
        Assert.IsNotNull(decision);
        Assert.AreEqual(5, decision!.TriageLevel);
    }

    [TestMethod]
    public async Task Decide_WithCriticalBleeding_ReturnsLevelOne()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.NurseEmail);

        var response = await client.PostAsJsonAsync("/api/triage/decide", BuildParameters(1, 1, 3, 1, 1));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var decision = await response.Content.ReadFromJsonAsync<TriageDecisionResponse>(JsonOptions);
        Assert.IsNotNull(decision);
        Assert.AreEqual(1, decision!.TriageLevel);
    }

    [TestMethod]
    public async Task Decide_WithOutOfRangeParameters_ReturnsBadRequest()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.PostAsJsonAsync("/api/triage/decide", BuildParameters(9, 1, 1, 1, 1));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Update_Missing_ReturnsNotFound()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.PutAsJsonAsync("/api/triage/999999", BuildTriage());

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Delete_Missing_ReturnsNotFound()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.DeleteAsync("/api/triage/999999");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    // The [ApiController] attribute treats non-nullable navigation properties as
    // implicitly required, so request bodies must include the full object graph
    // (TriageParameters -> Triage -> Visit -> Patient) to pass model validation.
    private static object BuildParameters(int consciousness, int breathing, int bleeding, int injuryType, int painLevel)
        => new
        {
            Consciousness = consciousness,
            Breathing = breathing,
            Bleeding = bleeding,
            InjuryType = injuryType,
            PainLevel = painLevel,
            Triage = BuildTriage(),
        };

    private static object BuildTriage()
        => new
        {
            TriageLevel = 3,
            Specialization = "Cardiology",
            NurseId = 1,
            Visit = new
            {
                ChiefComplaint = "Chest pain",
                Status = "REGISTERED",
                Patient = new
                {
                    FirstName = "Test",
                    LastName = "Patient",
                    Cnp = "1900101123456",
                    PhoneNumber = "0700000000",
                    EmergencyContact = "Contact 0700000000",
                },
            },
        };
}
using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class AddictDetectionEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetCandidates_AsDoctor_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/addicts/candidates");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var candidates = await response.Content.ReadFromJsonAsync<List<Patient>>(JsonOptions);
        Assert.IsNotNull(candidates);
    }

    [TestMethod]
    public async Task GetCandidates_AsPharmacist_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var response = await client.GetAsync("/api/addicts/candidates");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetChronicConditions_ForPatientWithoutHistory_ReturnsNoneReported()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        // Archived patient was seeded with a MedicalHistory but no chronic
        // conditions, so the service returns the "None reported." sentinel.
        var response = await client.GetAsync($"/api/addicts/{Ids.ArchivedPatientId}/chronic-conditions");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.AreEqual("None reported.", text);
    }

    [TestMethod]
    public async Task GetChronicConditions_ForPatientWithConditions_ReturnsThem()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        // Active patient was seeded with a chronic condition of "Asthma".
        var response = await client.GetAsync($"/api/addicts/{Ids.ActivePatientId}/chronic-conditions");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.AreEqual("Asthma", text);
    }

    [TestMethod]
    public async Task GetChronicConditions_WithInvalidId_ReturnsBadRequest()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        // patientId <= 0 trips the ArgumentException guard mapped to 400.
        var response = await client.GetAsync("/api/addicts/0/chronic-conditions");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task MarkNotified_ValidPatient_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.PostAsync($"/api/addicts/{Ids.ActivePatientId}/notify", content: null);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task BuildPoliceReport_UnknownPatient_ReturnsBadRequest()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        // No prescriptions exist for this patient, so the service cannot resolve
        // the patient and throws ArgumentException -> mapped to 400.
        var response = await client.PostAsJsonAsync("/api/addicts/police-report",
            new BuildPoliceReportRequest { PatientId = Ids.ActivePatientId });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

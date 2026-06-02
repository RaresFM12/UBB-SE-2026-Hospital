using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Nurse")]
[Route("api/patients")]
public class PatientsController(IPatientService patientService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Patient>>> GetAll(CancellationToken cancellationToken)
        => Ok(await patientService.GetPatientsAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Patient>> GetById(int id)
    {
        Patient? patient = await patientService.GetByIdAsync(id);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpGet("{id:int}/details")]
    public async Task<ActionResult<Patient>> GetDetails(int id)
    {
        try { return Ok(await patientService.GetPatientDetailsAsync(id)); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
    }

    [HttpGet("{id:int}/medical-history")]
    public async Task<ActionResult<MedicalHistory>> GetMedicalHistory(int id)
    {
        MedicalHistory? history = await patientService.GetMedicalHistoryAsync(id);
        return history is null ? NotFound() : Ok(history);
    }

    [HttpGet("{historyId:int}/medical-records")]
    public async Task<ActionResult<List<MedicalRecord>>> GetMedicalRecords(int historyId)
        => Ok(await patientService.GetMedicalRecordsAsync(historyId));

    [HttpPost("{patientId:int}/medical-records")]
    public async Task<ActionResult<int>> CreateMedicalRecord(int patientId, [FromBody] CreateMedicalRecordRequest dto)
    {
        try { return Ok(await patientService.CreateMedicalRecordAsync(patientId, dto)); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
    }

    [HttpGet("{id:int}/allergies")]
    public async Task<ActionResult<List<string>>> GetAllergies(int id)
        => Ok(await patientService.GetPatientAllergiesAsync(id));

    [HttpGet("records/{recordId:int}/prescription")]
    public async Task<ActionResult<Prescription>> GetPrescription(int recordId)
    {
        Prescription? prescription = await patientService.GetPrescriptionByRecordIdAsync(recordId);
        return prescription is null ? NotFound() : Ok(prescription);
    }

    [HttpGet("{id:int}/high-risk")]
    public async Task<ActionResult<bool>> IsHighRisk(int id)
        => Ok(await patientService.IsHighRiskPatientAsync(id));

    [HttpGet("exists/{cnp}")]
    public async Task<ActionResult<bool>> Exists(string cnp)
        => Ok(await patientService.ExistsAsync(cnp));

    [HttpPost("search")]
    public async Task<ActionResult<List<Patient>>> Search([FromBody] SearchPatientsRequest dto)
        => Ok(await patientService.SearchPatientsAsync(dto));

    [HttpPost]
    public async Task<ActionResult<Patient>> Create([FromBody] CreatePatientRequest dto)
    {
        Patient patient = await patientService.CreatePatientAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = patient.PatientId }, patient);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientRequest dto)
    {
        try { await patientService.UpdatePatientAsync(id, dto); return NoContent(); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
    }

    [HttpPut("{id:int}/archive")]
    public async Task<IActionResult> Archive(int id)
    {
        try { await patientService.ArchivePatientAsync(id); return NoContent(); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
    }

    [HttpPut("{id:int}/dearchive")]
    public async Task<IActionResult> Dearchive(int id)
    {
        try { await patientService.DearchivePatientAsync(id); return NoContent(); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
    }

    [HttpPut("{id:int}/archive-deceased")]
    public async Task<IActionResult> ArchiveDeceased(int id, [FromBody] ArchiveAsDeceasedRequest dto)
    {
        try { await patientService.ArchiveAsDeceasedAsync(id, dto); return NoContent(); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
    }

    [HttpPost("{id:int}/medical-history")]
    public async Task<IActionResult> CreateMedicalHistory(int id, [FromBody] CreateMedicalHistoryRequest dto)
    {
        try { await patientService.CreateMedicalHistoryAsync(id, dto); return NoContent(); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await patientService.DeletePatientAsync(id);
        return NoContent();
    }
}

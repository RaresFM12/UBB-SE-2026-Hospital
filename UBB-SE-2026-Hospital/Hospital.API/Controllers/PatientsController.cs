using Hospital.Data.Models;
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

    [HttpPost("search")]
    public async Task<ActionResult<List<Patient>>> Search([FromBody] SearchPatientsRequest? request, CancellationToken cancellationToken)
        => Ok(await patientService.SearchPatientsAsync(request, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Patient>> GetById(int id, CancellationToken cancellationToken)
    {
        Patient? patient = await patientService.GetByIdAsync(id, cancellationToken);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpGet("{id:int}/details")]
    public async Task<ActionResult<Patient>> GetDetails(int id, CancellationToken cancellationToken)
        => Ok(await patientService.GetPatientDetailsAsync(id, cancellationToken));

    [HttpGet("{id:int}/medical-history")]
    public async Task<ActionResult<MedicalHistory>> GetMedicalHistory(int id)
    {
        MedicalHistory? history = await patientService.GetMedicalHistoryAsync(id);
        return history is null ? NotFound() : Ok(history);
    }

    [HttpPost("{id:int}/medical-history")]
    public async Task<IActionResult> CreateMedicalHistory(int id, [FromBody] CreateMedicalHistoryRequest request, CancellationToken cancellationToken)
    {
        await patientService.CreateMedicalHistoryAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpGet("{historyId:int}/medical-records")]
    public async Task<ActionResult<List<MedicalRecord>>> GetMedicalRecords(int historyId)
        => Ok(await patientService.GetMedicalRecordsAsync(historyId));

    [HttpPost("{patientId:int}/medical-records")]
    public async Task<ActionResult<int>> CreateMedicalRecord(int patientId, [FromBody] CreateMedicalRecordRequest request, CancellationToken cancellationToken)
    {
        var record = new MedicalRecord
        {
            SourceType = request.SourceType,
            SourceId = request.SourceId,
            StaffMember = new Staff { StaffId = request.StaffId },
            Symptoms = request.Symptoms,
            Diagnosis = request.Diagnosis,
            ConsultationDate = request.ConsultationDate,
            BasePrice = request.BasePrice,
            FinalPrice = request.FinalPrice,
            PoliceNotified = request.PoliceNotified,
        };
        int recordId = await patientService.CreateMedicalRecordAsync(patientId, record, cancellationToken);
        return Ok(recordId);
    }

    [HttpGet("{id:int}/allergies")]
    public async Task<ActionResult<List<string>>> GetAllergies(int id, CancellationToken cancellationToken)
        => Ok(await patientService.GetPatientAllergiesAsync(id, cancellationToken));

    [HttpGet("{id:int}/high-risk")]
    public async Task<ActionResult<bool>> IsHighRisk(int id, CancellationToken cancellationToken)
        => Ok(await patientService.IsHighRiskPatientAsync(id, cancellationToken));

    [HttpGet("exists/{cnp}")]
    public async Task<ActionResult<bool>> Exists(string cnp)
        => Ok(await patientService.ExistsAsync(cnp));

    [HttpPost]
    public async Task<ActionResult<Patient>> Create([FromBody] CreatePatientRequest request, CancellationToken cancellationToken)
        => Ok(await patientService.CreatePatientAsync(request, cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientRequest request, CancellationToken cancellationToken)
    {
        await patientService.UpdatePatientAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:int}/archive")]
    public async Task<IActionResult> Archive(int id, CancellationToken cancellationToken)
    {
        await patientService.ArchivePatientAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:int}/dearchive")]
    public async Task<IActionResult> Dearchive(int id, CancellationToken cancellationToken)
    {
        await patientService.DearchivePatientAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:int}/archive-deceased")]
    public async Task<IActionResult> ArchiveAsDeceased(int id, [FromBody] ArchiveAsDeceasedRequest request, CancellationToken cancellationToken)
    {
        await patientService.ArchiveAsDeceasedAsync(id, request.DeathDate, cancellationToken);
        return NoContent();
    }

    [HttpGet("records/{recordId:int}/prescription")]
    public async Task<ActionResult<Prescription>> GetPrescriptionByRecord(int recordId, CancellationToken cancellationToken)
    {
        Prescription? prescription = await patientService.GetPrescriptionByRecordIdAsync(recordId, cancellationToken);
        return prescription is null ? NotFound() : Ok(prescription);
    }

    [HttpPost("records/{recordId:int}/prescription")]
    public async Task<IActionResult> CreatePrescription(int recordId, [FromBody] CreatePrescriptionRequest request)
    {
        var prescription = new Prescription
        {
            DoctorNotes = request.DoctorNotes,
            Date = request.Date,
            MedicationList = request.Items
                .Select(item => new PrescriptionItem { MedicationName = item.MedicationName, Quantity = item.Quantity })
                .ToList(),
        };
        await patientService.CreatePrescriptionAsync(recordId, prescription);
        return NoContent();
    }

    [HttpGet("records/{recordId:int}/export-data")]
    public async Task<ActionResult<RecordExportDataDto>> GetRecordExportData(int recordId, CancellationToken cancellationToken)
        => Ok(await patientService.GetRecordExportDataAsync(recordId, cancellationToken));
}

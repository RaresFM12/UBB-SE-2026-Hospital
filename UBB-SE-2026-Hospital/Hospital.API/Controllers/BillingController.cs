#if false
using Hospital.Data.Models.DTOs;
using Hospital.Services.PatientEr;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Pharmacist")]
[Route("api/billing")]
public class BillingController(IBillingService billingService) : ControllerBase
{
    [HttpGet("base-price/{patientId:int}/{recordId:int}")]
    public async Task<ActionResult<decimal>> ComputeBasePrice([FromRoute] int patientId, [FromRoute] int recordId)
    {
        try
        {
            return Ok(await billingService.ComputeBasePriceAsync(patientId, recordId));
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500, title: "Could not compute base price.");
        }
    }

    [HttpPost("discount")]
    public async Task<ActionResult<decimal>> ApplyDiscount([FromBody] ApplyDiscountRequest dto)
    {
        try
        {
            return Ok(await billingService.ApplyDiscountAsync(dto.BasePrice, dto.Discount));
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500, title: "Could not apply discount.");
        }
    }

    [HttpPost("discount/{recordId:int}")]
    public async Task<ActionResult<decimal>> PersistDiscount([FromRoute] int recordId, [FromBody] ApplyDiscountRequest dto)
    {
        try
        {
            return Ok(await billingService.PersistDiscountAsync(recordId, dto.BasePrice, dto.Discount));
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(detail: ex.Message, statusCode: 404, title: "Medical record not found.");
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500, title: "Could not persist discount.");
        }
    }
}
#endif

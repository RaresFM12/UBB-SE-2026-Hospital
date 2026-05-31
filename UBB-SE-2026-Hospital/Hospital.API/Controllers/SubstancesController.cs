using Hospital.Data.Models;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[Authorize]
[Route("api/substances")]
public class SubstancesController(IAdminService adminService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Substance>>> GetAll(CancellationToken cancellationToken = default)
        => Ok(await adminService.GetSubstancesAsync(cancellationToken));

    [HttpGet("{name}")]
    public async Task<ActionResult<Substance>> GetByName(string name, CancellationToken cancellationToken = default)
    {
        if (!await adminService.SubstanceExistsAsync(name, cancellationToken))
            return NotFound();
        return Ok(await adminService.GetSubstanceByNameAsync(name, cancellationToken));
    }

    [HttpGet("{name}/exists")]
    public async Task<ActionResult<bool>> Exists(string name, CancellationToken cancellationToken = default)
        => Ok(await adminService.SubstanceExistsAsync(name, cancellationToken));

    [HttpGet("top")]
    public async Task<ActionResult<Dictionary<string, int>>> GetTop(CancellationToken cancellationToken = default)
        => Ok(await adminService.GetTopSubstancesAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubstanceRequest request, CancellationToken cancellationToken = default)
    {
        await adminService.CreateSubstanceAsync(request.Name, request.LethalDose, request.Description, cancellationToken);
        return NoContent();
    }

    [HttpPut("{name}")]
    public async Task<IActionResult> Update(string name, [FromBody] Substance substance, CancellationToken cancellationToken = default)
    {
        substance.Name = name;
        await adminService.UpdateSubstanceAsync(substance, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> Delete(string name, CancellationToken cancellationToken = default)
    {
        await adminService.DeleteSubstanceAsync(name, cancellationToken);
        return NoContent();
    }

    public record CreateSubstanceRequest(string Name, float LethalDose, string Description);
}

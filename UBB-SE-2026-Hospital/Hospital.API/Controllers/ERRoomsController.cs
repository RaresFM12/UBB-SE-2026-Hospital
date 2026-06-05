using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Nurse","ERDoctor")]
[Route("api/errooms")]
public class ERRoomsController(IERRoomService erRoomService, ILogger<ERRoomsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ERRoom>>> GetAll()
    {
        try { return Ok(await erRoomService.GetAllAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch ER rooms."); return Problem(statusCode: 500, title: "Could not fetch ER rooms."); }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ERRoom>> GetById(int id)
    {
        try
        {
            ERRoom? result = await erRoomService.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch ER room {Id}.", id); return Problem(statusCode: 500, title: "Could not fetch ER room."); }
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<List<ERRoom>>> GetByStatus(string status)
    {
        try { return Ok(await erRoomService.GetByStatusAsync(status)); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch ER rooms by status {Status}.", status); return Problem(statusCode: 500, title: "Could not fetch ER rooms by status."); }
    }

    [HttpGet("{id:int}/visit-details")]
    public async Task<ActionResult<ERRoomVisitDetails>> GetVisitDetails(int id)
    {
        try
        {
            ERRoomVisitDetails? result = await erRoomService.GetVisitDetailsAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch visit details for room {Id}.", id); return Problem(statusCode: 500, title: "Could not fetch room visit details."); }
    }

    [HttpPost]
    public async Task<ActionResult<ERRoom>> Create([FromBody] ERRoom room)
    {
        try
        {
            ERRoom result = await erRoomService.CreateAsync(room);
            return CreatedAtAction(nameof(GetById), new { id = result.RoomId }, result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to create ER room."); return Problem(statusCode: 500, title: "Could not create ER room."); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ERRoom room)
    {
        try
        {
            room.RoomId = id;
            await erRoomService.UpdateAsync(room);
            return NoContent();
        }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to update ER room {Id}.", id); return Problem(statusCode: 500, title: "Could not update ER room."); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            ERRoom? room = await erRoomService.GetByIdAsync(id);
            if (room is null) return NotFound();

            await erRoomService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to delete ER room {Id}.", id); return Problem(statusCode: 500, title: "Could not delete ER room."); }
    }

    [HttpPost("{id:int}/mark-cleaning")]
    public async Task<IActionResult> MarkCleaning(int id)
    {
        try { await erRoomService.MarkRoomAsCleaningAsync(id); return NoContent(); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to mark ER room {Id} as cleaning.", id); return Problem(statusCode: 500, title: "Could not mark ER room as cleaning."); }
    }

    [HttpPost("{id:int}/mark-available")]
    public async Task<IActionResult> MarkAvailable(int id)
    {
        try { await erRoomService.MarkRoomAsAvailableAsync(id); return NoContent(); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to mark ER room {Id} as available.", id); return Problem(statusCode: 500, title: "Could not mark ER room as available."); }
    }
}

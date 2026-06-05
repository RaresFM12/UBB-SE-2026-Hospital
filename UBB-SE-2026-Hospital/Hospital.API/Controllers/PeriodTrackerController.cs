using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin", "Client")]
[Route("api/period-tracker")]
public class PeriodTrackerController(IPeriodTrackerService periodTrackerService) : ControllerBase
{
    [HttpGet("{userId:int}/user")]
    public async Task<ActionResult<User>> GetUser(int userId, CancellationToken cancellationToken = default)
        => Ok(await periodTrackerService.GetUserAsync(userId, cancellationToken));

    [HttpGet("{userId:int}/state")]
    public ActionResult<PeriodTrackerState> GetState(int userId)
        => Ok(periodTrackerService.GetTrackerState(userId));

    [HttpGet("{userId:int}/dashboard")]
    public ActionResult<PeriodTrackerDashboardSnapshot> GetDashboard(int userId, [FromQuery] int monthOffset = 0)
        => Ok(periodTrackerService.GetDashboardSnapshot(userId, monthOffset));

    [HttpGet("{userId:int}/notes")]
    public async Task<ActionResult<IReadOnlyList<PeriodTrackerNoteDto>>> GetNotes(int userId, CancellationToken cancellationToken = default)
    {
        Dictionary<int, (string Body, bool IsDone)> notes = await periodTrackerService.GetNotesAsync(userId, cancellationToken);
        return Ok(notes
            .Select(noteEntry => new PeriodTrackerNoteDto
            {
                NoteId = noteEntry.Key,
                Body = noteEntry.Value.Body,
                IsDone = noteEntry.Value.IsDone,
            })
            .ToList());
    }

    [HttpPut("{userId:int}")]
    public async Task<IActionResult> UpdateTracker(int userId, [FromBody] UpdatePeriodTrackerRequest request, CancellationToken cancellationToken = default)
    {
        await periodTrackerService.UpdatePeriodTrackerAsync(userId, request.StartPeriodDate, request.CycleDays, request.PeriodLasts, request.PremenstrualSyndromeOption, cancellationToken);
        return NoContent();
    }

    [HttpPost("{userId:int}/notes")]
    public async Task<IActionResult> AddNote(int userId, [FromBody] AddPeriodTrackerNoteRequest request, CancellationToken cancellationToken = default)
    {
        await periodTrackerService.AddNoteAsync(userId, request.NoteBody, cancellationToken);
        return NoContent();
    }

    [HttpPut("{userId:int}/notes/{noteId:int}")]
    public async Task<IActionResult> UpdateNote(int userId, int noteId, [FromBody] UpdatePeriodTrackerNoteRequest request, CancellationToken cancellationToken = default)
    {
        await periodTrackerService.UpdateNoteAsync(userId, noteId, request.NoteBody, request.IsDone, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{userId:int}/notes/{noteId:int}")]
    public async Task<IActionResult> DeleteNote(int userId, int noteId, CancellationToken cancellationToken = default)
    {
        await periodTrackerService.DeleteNoteAsync(userId, noteId, cancellationToken);
        return NoContent();
    }

}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

/// <summary>
/// Backs the Web "Ghost Sighting Monitor" easter-egg feature.
/// Sightings are kept in memory only (no database) — a sighting "expires"
/// after 24 hours, and an exorcism is triggered when more than three are
/// reported within that window.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/ghost")]
public class GhostController : ControllerBase
{
    private const int ExorcismThreshold = 3;

    private static readonly List<DateTime> Sightings = new();
    private static readonly object SyncRoot = new();

    [HttpPost("sighting")]
    public ActionResult<GhostStatus> ReportSighting()
    {
        lock (SyncRoot)
        {
            Sightings.Add(DateTime.UtcNow);
            return Ok(BuildStatus());
        }
    }

    [HttpGet("exorcism-status")]
    public ActionResult<GhostStatus> GetExorcismStatus()
    {
        lock (SyncRoot)
        {
            return Ok(BuildStatus());
        }
    }

    private static GhostStatus BuildStatus()
    {
        DateTime cutoff = DateTime.UtcNow.AddHours(-24);
        Sightings.RemoveAll(timestamp => timestamp < cutoff);

        int sightingCount = Sightings.Count;
        bool exorcismTriggered = sightingCount > ExorcismThreshold;
        return new GhostStatus(exorcismTriggered, sightingCount);
    }

    public record GhostStatus(bool exorcismTriggered, int sightingCount);
}

using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Pharmacist","Nurse")]
[Route("api/notifications")]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        await notificationService.CreateNotificationAsync(request.RecipientStaffId, request.Title, request.Message, cancellationToken);
        return NoContent();
    }

    public record CreateNotificationRequest(int RecipientStaffId, string Title, string Message);
}

using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin", "Doctor")]
[Route("api/wellness-items")]
public class WellnessItemsController(IWellnessItemsService wellnessItemsService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Item>> GetAll()
        => Ok(wellnessItemsService.GetWellnessItems());
}

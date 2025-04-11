using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ChecklistController : ControllerBase
{
    [HttpGet]
    public IActionResult GetChecklist()
    {
        return Ok(new[] { "Task 1", "Task 2" }); // Mock data
    }
}
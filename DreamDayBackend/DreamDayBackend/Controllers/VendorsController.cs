using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class VendorsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetVendors()
    {
        return Ok(new[] { "Vendor 1", "Vendor 2" }); // Mock data
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DreamDayBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetDashboard()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId == null) return Unauthorized();

            // Mock data for now; replace with real data later
            var dashboardData = new
            {
                Todos = new[] { "Book venue", "Send invites" },
                BudgetOverview = new { Total = 10000, Spent = 5000 },
                KeyDates = new[] { "2025-12-25", "2025-12-01" },
                Role = role
            };

            return Ok(dashboardData);
        }
    }
}
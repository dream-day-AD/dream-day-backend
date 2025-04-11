using DreamDayBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DreamDayBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GuestsController : ControllerBase
    {
        private readonly DreamDayDbContext _context;

        public GuestsController(DreamDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetGuests()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var guests = await _context.Guests
                .Where(g => g.UserId == userId)
                .ToListAsync();
            return Ok(guests);
        }

        [HttpPost]
        public async Task<IActionResult> AddGuest([FromBody] Guest guest)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            guest.UserId = userId;
            _context.Guests.Add(guest);
            await _context.SaveChangesAsync();
            return Ok(guest);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateGuest(int id, [FromBody] Guest updates)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var guest = await _context.Guests
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
            if (guest == null) return NotFound();

            if (updates.RSVP != null) guest.RSVP = updates.RSVP;
            if (updates.MealPreference != null) guest.MealPreference = updates.MealPreference;
            if (updates.Seating != null) guest.Seating = updates.Seating;

            await _context.SaveChangesAsync();
            return Ok(guest);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGuest(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var guest = await _context.Guests
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
            if (guest == null) return NotFound();

            _context.Guests.Remove(guest);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Guest deleted" });
        }
    }
}
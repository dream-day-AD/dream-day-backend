using DreamDayBackend.Models;
using DreamDayBackend.Dtos;
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
        public async Task<ActionResult<IEnumerable<Guest>>> GetGuests()
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (email == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return Unauthorized("User not found in database.");
            }

            // If user is admin or planner, return all guests
            if (User.IsInRole("admin") || User.IsInRole("planner"))
            {
                return await _context.Guests.ToListAsync();
            }

            // For clients, only return their own guests
            return await _context.Guests
                .Where(g => g.UserId == user.Id)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Guest>> GetGuest(Guid id)
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (email == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return Unauthorized("User not found in database.");
            }

            Guest guest;

            // If user is admin or planner, can get any guest
            if (User.IsInRole("admin") || User.IsInRole("planner"))
            {
                guest = await _context.Guests.FindAsync(id);
            }
            else
            {
                // For clients, only get their own guests
                guest = await _context.Guests
                    .FirstOrDefaultAsync(g => g.Id == id && g.UserId == user.Id);
            }

            if (guest == null)
            {
                return NotFound("Guest not found or you don't have access to it.");
            }

            return guest;
        }

        [HttpPost]
        public async Task<ActionResult<Guest>> CreateGuest(GuestDto guestDto)
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (email == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return Unauthorized("User not found in database.");
            }

            var guest = new Guest
            {
                Id = Guid.NewGuid(),
                Name = guestDto.Name,
                Email = guestDto.Email,
                RSVP = string.IsNullOrEmpty(guestDto.RSVP) ? "Pending" : guestDto.RSVP,
                MealPreference = string.IsNullOrEmpty(guestDto.MealPreference) ? "None" : guestDto.MealPreference,
                Seating = guestDto.Seating ?? string.Empty,
                UserId = user.Id // Use the actual user ID from the database
            };

            try
            {
                _context.Guests.Add(guest);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetGuest), new { id = guest.Id }, guest);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Failed to create guest: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGuest(Guid id, GuestDto guestDto)
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (email == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return Unauthorized("User not found in database.");
            }

            Guest guest;

            // If user is admin or planner, can update any guest
            if (User.IsInRole("admin") || User.IsInRole("planner"))
            {
                guest = await _context.Guests.FindAsync(id);
            }
            else
            {
                // For clients, only update their own guests
                guest = await _context.Guests
                    .FirstOrDefaultAsync(g => g.Id == id && g.UserId == user.Id);
            }

            if (guest == null)
            {
                return NotFound("Guest not found or you don't have access to it.");
            }

            guest.Name = guestDto.Name;
            guest.Email = guestDto.Email;
            guest.RSVP = string.IsNullOrEmpty(guestDto.RSVP) ? guest.RSVP : guestDto.RSVP;
            guest.MealPreference = string.IsNullOrEmpty(guestDto.MealPreference) ? guest.MealPreference : guestDto.MealPreference;
            guest.Seating = guestDto.Seating ?? guest.Seating;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Failed to update guest: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchGuest(Guid id, GuestDto guestDto)
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (email == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return Unauthorized("User not found in database.");
            }

            Guest guest;

            // If user is admin or planner, can update any guest
            if (User.IsInRole("admin") || User.IsInRole("planner"))
            {
                guest = await _context.Guests.FindAsync(id);
            }
            else
            {
                // For clients, only update their own guests
                guest = await _context.Guests
                    .FirstOrDefaultAsync(g => g.Id == id && g.UserId == user.Id);
            }

            if (guest == null)
            {
                return NotFound("Guest not found or you don't have access to it.");
            }

            // Only update fields that are provided
            if (!string.IsNullOrEmpty(guestDto.Name))
            {
                guest.Name = guestDto.Name;
            }

            if (!string.IsNullOrEmpty(guestDto.Email))
            {
                guest.Email = guestDto.Email;
            }

            if (!string.IsNullOrEmpty(guestDto.RSVP))
            {
                guest.RSVP = guestDto.RSVP;
            }

            if (!string.IsNullOrEmpty(guestDto.MealPreference))
            {
                guest.MealPreference = guestDto.MealPreference;
            }

            if (guestDto.Seating != null)
            {
                guest.Seating = guestDto.Seating;
            }

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Failed to update guest: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGuest(Guid id)
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (email == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return Unauthorized("User not found in database.");
            }

            Guest guest;

            // If user is admin or planner, can delete any guest
            if (User.IsInRole("admin") || User.IsInRole("planner"))
            {
                guest = await _context.Guests.FindAsync(id);
            }
            else
            {
                // For clients, only delete their own guests
                guest = await _context.Guests
                    .FirstOrDefaultAsync(g => g.Id == id && g.UserId == user.Id);
            }

            if (guest == null)
            {
                return NotFound("Guest not found or you don't have access to it.");
            }

            try
            {
                _context.Guests.Remove(guest);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Failed to delete guest: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}

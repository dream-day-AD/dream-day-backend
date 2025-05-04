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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.Guests
                .Where(g => g.UserId == userId)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Guest>> GetGuest(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var guest = await _context.Guests
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (guest == null)
            {
                return NotFound();
            }
            return guest;
        }

        [HttpPost]
        public async Task<ActionResult<Guest>> CreateGuest(GuestDto guestDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var guest = new Guest
            {
                Id = Guid.NewGuid(),
                Name = guestDto.Name,
                Email = guestDto.Email,
                RSVP = guestDto.RSVP,
                MealPreference = guestDto.MealPreference,
                Seating = guestDto.Seating,
                UserId = userId
            };

            _context.Guests.Add(guest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGuest), new { id = guest.Id }, guest);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGuest(Guid id, GuestDto guestDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var guest = await _context.Guests
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (guest == null)
            {
                return NotFound();
            }

            guest.Name = guestDto.Name;
            guest.Email = guestDto.Email;
            guest.RSVP = guestDto.RSVP;
            guest.MealPreference = guestDto.MealPreference;
            guest.Seating = guestDto.Seating;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGuest(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var guest = await _context.Guests
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (guest == null)
            {
                return NotFound();
            }

            _context.Guests.Remove(guest);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

using DreamDayBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DreamDayBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GuestController : ControllerBase
    {
        private readonly DreamDayDbContext _context;

        public GuestController(DreamDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Guest>>> GetGuests()
        {
            return await _context.Guests.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Guest>> GetGuest(Guid id)
        {
            var guest = await _context.Guests.FindAsync(id);
            if (guest == null)
            {
                return NotFound();
            }
            return guest;
        }

        [HttpPost]
        public async Task<ActionResult<Guest>> CreateGuest(Guest guest)
        {
            _context.Guests.Add(guest);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGuest), new { id = guest.Id }, guest);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGuest(Guid id, Guest guest)
        {
            if (id != guest.Id) // Fixed comparison (Guid to Guid)
            {
                return BadRequest();
            }

            var existingGuest = await _context.Guests.FindAsync(id);
            if (existingGuest == null)
            {
                return NotFound();
            }

            existingGuest.Name = guest.Name;
            existingGuest.RSVP = guest.RSVP; // Now valid
            existingGuest.MealPreference = guest.MealPreference; // Now valid
            existingGuest.Seating = guest.Seating; // Now valid

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGuest(Guid id)
        {
            var guest = await _context.Guests.FindAsync(id);
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
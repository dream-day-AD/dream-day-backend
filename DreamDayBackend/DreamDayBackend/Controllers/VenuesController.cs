using DreamDayBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DreamDayBackend.Controllers
{
    [Authorize] // Requires authentication
    [Route("api/[controller]")]
    [ApiController]
    public class VenuesController : ControllerBase
    {
        private readonly DreamDayDbContext _context;

        public VenuesController(DreamDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Venues
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venue>>> GetVenues()
        {
            return await _context.Venues.ToListAsync();
        }

        // GET: api/Venues/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Venue>> GetVenue(Guid id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound("Venue not found.");
            }
            return venue;
        }

        // POST: api/Venues
        [HttpPost]
        [Authorize(Roles = "admin")] // Only admins can create venues
        public async Task<ActionResult<Venue>> CreateVenue(Venue venue)
        {
            venue.VenueId = Guid.NewGuid();
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetVenue), new { id = venue.VenueId }, venue);
        }

        // PUT: api/Venues/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")] // Only admins can update venues
        public async Task<IActionResult> UpdateVenue(Guid id, Venue venue)
        {
            if (id != venue.VenueId)
            {
                return BadRequest("Venue ID mismatch.");
            }

            var existingVenue = await _context.Venues.FindAsync(id);
            if (existingVenue == null)
            {
                return NotFound("Venue not found.");
            }

            existingVenue.Name = venue.Name;
            existingVenue.Address = venue.Address;
            existingVenue.Capacity = venue.Capacity;
            existingVenue.Price = venue.Price;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Venues/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // Only admins can delete venues
        public async Task<IActionResult> DeleteVenue(Guid id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound("Venue not found.");
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
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
    public class EventsController : ControllerBase
    {
        private readonly DreamDayDbContext _context;

        public EventsController(DreamDayDbContext context)
        {
            _context = context;
        }

        // Map Event to EventResponseDto
        private EventResponseDto MapToResponseDto(Event evt)
        {
            return new EventResponseDto
            {
                EventId = evt.EventId,
                UserId = evt.UserId,
                EventName = evt.EventName,
                Date = evt.Date,
                Time = evt.Time,
                Description = evt.Description,
                VenueId = evt.VenueId,
                Venue = evt.Venue != null ? new VenueResponseDto
                {
                    VenueId = evt.Venue.VenueId,
                    Name = evt.Venue.Name,
                    Address = evt.Venue.Address,
                    Capacity = evt.Venue.Capacity,
                    Price = evt.Venue.Price
                } : null
            };
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetEvents()
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

            var events = await _context.Events
                .Where(e => e.UserId == user.Id)
                .Include(e => e.Venue)
                .ToListAsync();

            return Ok(events.Select(MapToResponseDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventResponseDto>> GetEvent(Guid id)
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

            var evt = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventId == id && e.UserId == user.Id);

            if (evt == null)
            {
                return NotFound("Event not found or you don't have access to it.");
            }

            return Ok(MapToResponseDto(evt));
        }

        [HttpPost]
        public async Task<ActionResult<EventResponseDto>> CreateEvent(EventDto eventDto)
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

            var venue = await _context.Venues.FindAsync(eventDto.VenueId);
            if (venue == null)
            {
                return BadRequest("Invalid VenueId: Venue does not exist.");
            }

            var evt = new Event
            {
                EventId = Guid.NewGuid(),
                UserId = user.Id,
                EventName = eventDto.EventName,
                Date = eventDto.Date,
                Time = eventDto.Time,
                Description = eventDto.Description,
                VenueId = eventDto.VenueId
            };

            _context.Events.Add(evt);
            await _context.SaveChangesAsync();

            // Fetch the event again with related data for the response
            var createdEvent = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventId == evt.EventId);

            return CreatedAtAction(nameof(GetEvent), new { id = evt.EventId }, MapToResponseDto(createdEvent));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(Guid id, EventDto eventDto)
        {
            if (id != eventDto.EventId)
            {
                return BadRequest("Event ID mismatch.");
            }

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

            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id && e.UserId == user.Id);
            if (existingEvent == null)
            {
                return NotFound("Event not found or you don't have access to it.");
            }

            var venue = await _context.Venues.FindAsync(eventDto.VenueId);
            if (venue == null)
            {
                return BadRequest("Invalid VenueId: Venue does not exist.");
            }

            existingEvent.EventName = eventDto.EventName;
            existingEvent.Date = eventDto.Date;
            existingEvent.Time = eventDto.Time;
            existingEvent.Description = eventDto.Description;
            existingEvent.VenueId = eventDto.VenueId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
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

            var evt = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id && e.UserId == user.Id);
            if (evt == null)
            {
                return NotFound("Event not found or you don't have access to it.");
            }

            _context.Events.Remove(evt);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
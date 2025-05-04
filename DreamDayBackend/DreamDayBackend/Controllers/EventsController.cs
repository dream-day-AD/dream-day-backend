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
            try
            {
                // Get email from the token's claim
                var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (email == null)
                {
                    return Unauthorized("User not authenticated.");
                }

                // Find the user in the database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return Unauthorized("User not found in database.");
                }

                // Check BOTH the database role and the claim role
                var dbRole = user.Role?.Trim().ToLower();
                var claimRole = User.FindFirstValue(ClaimTypes.Role)?.Trim().ToLower();

                Console.WriteLine($"Database role: {dbRole}, Claim role: {claimRole}");

                // If either role is admin or planner, show all events
                bool isAdminOrPlanner =
                    dbRole == "admin" || dbRole == "planner" ||
                    claimRole == "admin" || claimRole == "planner";

                var eventsQuery = _context.Events.AsQueryable();

                if (!isAdminOrPlanner)
                {
                    // Only filter by user ID if the user is a client
                    eventsQuery = eventsQuery.Where(e => e.UserId == user.Id);
                }

                var events = await eventsQuery
                    .Include(e => e.Venue)
                    .ToListAsync();

                Console.WriteLine($"Found {events.Count} events for user with role DB:{dbRole}/Claim:{claimRole}");

                return Ok(events.Select(MapToResponseDto));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetEvents: {ex}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("raw-test")]
        [AllowAnonymous] // No authentication required
        public async Task<ActionResult> GetRawEvents()
        {
            try
            {
                // Direct database access with minimal processing
                var rawEvents = await _context.Events
                    .Select(e => new
                    {
                        e.EventId,
                        e.EventName,
                        e.UserId,
                        e.Date,
                        e.Time,
                        e.Description,
                        e.VenueId,
                        VenueName = e.Venue != null ? e.Venue.Name : null
                    })
                    .ToListAsync();

                // Get count of events in database
                var count = await _context.Events.CountAsync();

                return Ok(new
                {
                    TotalCount = count,
                    RawEvents = rawEvents,
                    Message = "This is raw database data with no filtering"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = ex.Message,
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message
                });
            }
        }


        [HttpGet("debug-info")]
        public async Task<ActionResult> GetDebugInfo()
        {
            try
            {
                // Log all claims for debugging
                var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                Console.WriteLine("User claims: " + string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}")));

                var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var roleFromClaim = User.FindFirstValue(ClaimTypes.Role);
                Console.WriteLine($"Email from claim: {email}, Role from claim: {roleFromClaim}");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return NotFound("Current user not found in database");
                }

                Console.WriteLine($"User from DB - ID: {user.Id}, Role: {user.Role}");

                // Get raw events data without filtering
                var allEvents = await _context.Events.ToListAsync();
                var userEvents = await _context.Events.Where(e => e.UserId == user.Id).ToListAsync();

                return Ok(new
                {
                    CurrentUserEmail = email,
                    CurrentUserId = user.Id,
                    CurrentUserRole = user.Role,
                    RoleFromClaims = roleFromClaim,
                    AllClaims = claims,
                    TotalEventsCount = allEvents.Count,
                    UserEventsCount = userEvents.Count,
                    EventDetails = allEvents.Select(e => new {
                        EventId = e.EventId,
                        EventName = e.EventName,
                        UserId = e.UserId,
                        Date = e.Date,
                        VenueId = e.VenueId
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetDebugInfo: {ex}");
                return StatusCode(500, $"Error retrieving debug info: {ex.Message}");
            }
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<EventResponseDto>> GetEvent(Guid id)
        {
            try
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

                // Check user role
                var userRole = user.Role?.ToLower();

                Event evt;

                // Admin and planner can see any event
                if (userRole == "admin" || userRole == "planner")
                {
                    evt = await _context.Events
                        .Include(e => e.Venue)
                        .FirstOrDefaultAsync(e => e.EventId == id);
                }
                else
                {
                    // Regular users can only see their own events
                    evt = await _context.Events
                        .Include(e => e.Venue)
                        .FirstOrDefaultAsync(e => e.EventId == id && e.UserId == user.Id);
                }

                if (evt == null)
                {
                    return NotFound("Event not found or you don't have access to it.");
                }

                return Ok(MapToResponseDto(evt));
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetEvent: {ex}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<EventResponseDto>> CreateEvent(EventDto eventDto)
        {
            try
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
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in CreateEvent: {ex}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(Guid id, EventDto eventDto)
        {
            try
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

                Event existingEvent;
                var userRole = user.Role?.ToLower();

                // Admin and planner can update any event
                if (userRole == "admin" || userRole == "planner")
                {
                    existingEvent = await _context.Events.FindAsync(id);
                }
                else
                {
                    // Regular users can only update their own events
                    existingEvent = await _context.Events
                        .FirstOrDefaultAsync(e => e.EventId == id && e.UserId == user.Id);
                }

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
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in UpdateEvent: {ex}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            try
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

                Event evt;
                var userRole = user.Role?.ToLower();

                // Admin and planner can delete any event
                if (userRole == "admin" || userRole == "planner")
                {
                    evt = await _context.Events.FindAsync(id);
                }
                else
                {
                    // Regular users can only delete their own events
                    evt = await _context.Events
                        .FirstOrDefaultAsync(e => e.EventId == id && e.UserId == user.Id);
                }

                if (evt == null)
                {
                    return NotFound("Event not found or you don't have access to it.");
                }

                _context.Events.Remove(evt);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in DeleteEvent: {ex}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

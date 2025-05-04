using DreamDayBackend.Models;
using DreamDayBackend.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamDayBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly DreamDayDbContext _context;

        public BookingsController(DreamDayDbContext context)
        {
            _context = context;
        }

        // Helper method to check user role and access
        private async Task<(bool isAuthorized, ApplicationUser user, string message)> CheckUserAccessAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (email == null)
            {
                return (false, null, "User not authenticated.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return (false, null, "User not found in database.");
            }

            return (true, user, null);
        }

        // Map Booking to BookingResponseDto
        private BookingResponseDto MapToResponseDto(Booking booking)
        {
            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                EventId = booking.EventId,
                Status = booking.Status,
                TotalCost = booking.TotalCost,
                Event = booking.Event != null ? new EventResponseDto
                {
                    EventId = booking.Event.EventId,
                    UserId = booking.Event.UserId,
                    EventName = booking.Event.EventName,
                    Date = booking.Event.Date,
                    Time = booking.Event.Time,
                    Description = booking.Event.Description,
                    VenueId = booking.Event.VenueId,
                    Venue = booking.Event.Venue != null ? new VenueResponseDto
                    {
                        VenueId = booking.Event.Venue.VenueId,
                        Name = booking.Event.Venue.Name,
                        Address = booking.Event.Venue.Address,
                        Capacity = booking.Event.Venue.Capacity,
                        Price = booking.Event.Venue.Price
                    } : null
                } : null
            };
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetBookings()
        {
            var (isAuthorized, user, message) = await CheckUserAccessAsync();
            if (!isAuthorized)
            {
                return Unauthorized(message);
            }

            IQueryable<Booking> query;

            if (user.Role == "Client")
            {
                // For clients, first filter by user ID then include related data
                query = _context.Bookings
                    .Where(b => b.Event.UserId == user.Id)
                    .Include(b => b.Event)
                    .ThenInclude(e => e.Venue);
            }
            else
            {
                // For admin and planner, get all bookings with related data
                query = _context.Bookings
                    .Include(b => b.Event)
                    .ThenInclude(e => e.Venue);
            }

            var bookings = await query.ToListAsync();
            return Ok(bookings.Select(MapToResponseDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponseDto>> GetBooking(Guid id)
        {
            var (isAuthorized, user, message) = await CheckUserAccessAsync();
            if (!isAuthorized)
            {
                return Unauthorized(message);
            }

            Booking booking;

            if (user.Role == "Client")
            {
                // For clients, filter by booking ID and user ID
                booking = await _context.Bookings
                    .Include(b => b.Event)
                    .ThenInclude(e => e.Venue)
                    .FirstOrDefaultAsync(b => b.BookingId == id && b.Event.UserId == user.Id);
            }
            else
            {
                // For admin and planner, just filter by booking ID
                booking = await _context.Bookings
                    .Include(b => b.Event)
                    .ThenInclude(e => e.Venue)
                    .FirstOrDefaultAsync(b => b.BookingId == id);
            }

            if (booking == null)
            {
                return NotFound("Booking not found or you don't have access to it.");
            }

            return Ok(MapToResponseDto(booking));
        }

        [HttpPost]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking(BookingDto bookingDto)
        {
            var (isAuthorized, user, message) = await CheckUserAccessAsync();
            if (!isAuthorized)
            {
                return Unauthorized(message);
            }

            Event evt;

            if (user.Role == "Client")
            {
                // Clients can only book their own events
                evt = await _context.Events
                    .Include(e => e.Venue)
                    .FirstOrDefaultAsync(e => e.EventId == bookingDto.EventId && e.UserId == user.Id);
            }
            else
            {
                // Admins and Planners can book any event
                evt = await _context.Events
                    .Include(e => e.Venue)
                    .FirstOrDefaultAsync(e => e.EventId == bookingDto.EventId);
            }

            if (evt == null)
            {
                return BadRequest("Event not found or you don't have access to it.");
            }

            // Check for double-booking
            var conflictingBooking = await _context.Bookings
                .Include(b => b.Event)
                .AnyAsync(b => b.Event.VenueId == evt.VenueId && b.Event.Date == evt.Date);

            if (conflictingBooking)
            {
                return BadRequest("The venue is already booked for this date.");
            }

            // Get the venue for validation
            var venue = await _context.Venues.FindAsync(evt.VenueId);
            if (venue == null)
            {
                return BadRequest("Invalid VenueId: Venue does not exist.");
            }

            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = evt.EventId,
                Status = bookingDto.Status,
                TotalCost = bookingDto.TotalCost
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Fetch the booking again with related data for the response
            var createdBooking = await _context.Bookings
                .Include(b => b.Event)
                .ThenInclude(e => e.Venue)
                .FirstOrDefaultAsync(b => b.BookingId == booking.BookingId);

            return CreatedAtAction(nameof(GetBooking), new { id = booking.BookingId }, MapToResponseDto(createdBooking));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(Guid id, BookingDto bookingDto)
        {
            // Allow updating without specifying BookingId in the DTO
            if (bookingDto.BookingId != Guid.Empty && id != bookingDto.BookingId)
            {
                return BadRequest("Booking ID mismatch.");
            }

            var (isAuthorized, user, message) = await CheckUserAccessAsync();
            if (!isAuthorized)
            {
                return Unauthorized(message);
            }

            Booking existingBooking;

            if (user.Role == "Client")
            {
                // Clients can only update their own bookings
                existingBooking = await _context.Bookings
                    .Include(b => b.Event)
                    .FirstOrDefaultAsync(b => b.BookingId == id && b.Event.UserId == user.Id);
            }
            else
            {
                // Admins and Planners can update any booking
                existingBooking = await _context.Bookings
                    .Include(b => b.Event)
                    .FirstOrDefaultAsync(b => b.BookingId == id);
            }

            if (existingBooking == null)
            {
                return NotFound("Booking not found or you don't have access to it.");
            }

            // Update booking properties
            existingBooking.Status = bookingDto.Status;
            existingBooking.TotalCost = bookingDto.TotalCost;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(Guid id)
        {
            var (isAuthorized, user, message) = await CheckUserAccessAsync();
            if (!isAuthorized)
            {
                return Unauthorized(message);
            }

            Booking booking;

            if (user.Role == "Client")
            {
                // Clients can only delete their own bookings
                booking = await _context.Bookings
                    .Include(b => b.Event)
                    .FirstOrDefaultAsync(b => b.BookingId == id && b.Event.UserId == user.Id);
            }
            else
            {
                // Admins and Planners can delete any booking
                booking = await _context.Bookings
                    .Include(b => b.Event)
                    .FirstOrDefaultAsync(b => b.BookingId == id);
            }

            if (booking == null)
            {
                return NotFound("Booking not found or you don't have access to it.");
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool BookingExists(Guid id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}

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
    public class BookingsController : ControllerBase
    {
        private readonly DreamDayDbContext _context;

        public BookingsController(DreamDayDbContext context)
        {
            _context = context;
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

            var bookings = await _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e.Venue)
                .Where(b => b.Event.UserId == user.Id)
                .ToListAsync();

            return Ok(bookings.Select(MapToResponseDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponseDto>> GetBooking(Guid id)
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

            var booking = await _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e.Venue)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.Event.UserId == user.Id);

            if (booking == null)
            {
                return NotFound("Booking not found or you don't have access to it.");
            }

            return Ok(MapToResponseDto(booking));
        }

        [HttpPost]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking(BookingDto bookingDto)
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
                .FirstOrDefaultAsync(e => e.EventId == bookingDto.EventId && e.UserId == user.Id);
            if (evt == null)
            {
                return BadRequest("Event not found or you don't have access to it.");
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
            if (id != bookingDto.BookingId)
            {
                return BadRequest("Booking ID mismatch.");
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

            var existingBooking = await _context.Bookings
                .Include(b => b.Event)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.Event.UserId == user.Id);
            if (existingBooking == null)
            {
                return NotFound("Booking not found or you don't have access to it.");
            }

            existingBooking.Status = bookingDto.Status;
            existingBooking.TotalCost = bookingDto.TotalCost;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(Guid id)
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

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.Event.UserId == user.Id);
            if (booking == null)
            {
                return NotFound("Booking not found or you don't have access to it.");
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
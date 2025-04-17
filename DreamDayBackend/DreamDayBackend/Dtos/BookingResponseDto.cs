using System;

namespace DreamDayBackend.Dtos
{
    public class BookingResponseDto
    {
        public Guid BookingId { get; set; }
        public Guid EventId { get; set; }
        public string Status { get; set; }
        public decimal TotalCost { get; set; }

        // Include Event details as a DTO (optional, if you want to return event info)
        public EventResponseDto Event { get; set; }
    }
}
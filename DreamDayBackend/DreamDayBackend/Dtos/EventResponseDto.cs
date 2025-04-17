using System;

namespace DreamDayBackend.Dtos
{
    public class EventResponseDto
    {
        public Guid EventId { get; set; }
        public string UserId { get; set; }
        public string EventName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Description { get; set; }
        public Guid VenueId { get; set; }

        // Include Venue details as a DTO (optional, if you want to return venue info)
        public VenueResponseDto Venue { get; set; }
    }

    public class VenueResponseDto
    {
        public Guid VenueId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int Capacity { get; set; }
        public decimal Price { get; set; }
    }
}
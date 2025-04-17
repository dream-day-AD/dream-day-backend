using System;
using System.ComponentModel.DataAnnotations;

namespace DreamDayBackend.Dtos
{
    public class EventDto
    {
        public Guid EventId { get; set; } // Added EventId property

        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan Time { get; set; }

        public string Description { get; set; } = string.Empty;

        [Required]
        public Guid VenueId { get; set; }
    }
}
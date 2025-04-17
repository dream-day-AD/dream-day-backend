using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DreamDayBackend.Models;

namespace DreamDayBackend.Models
{
    public class Event
    {
        [Key]
        public Guid EventId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

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

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } // Removed [Required]

        [ForeignKey("VenueId")]
        public Venue Venue { get; set; } // Removed [Required]
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DreamDayBackend.Models
{
    public class Booking
    {
        [Key]
        public Guid BookingId { get; set; }

        [Required]
        public Guid EventId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty; // Initialized to empty string

        [Required]
        public decimal TotalCost { get; set; }

        [ForeignKey("EventId")]
        public Event Event { get; set; } // EF will handle initialization
    }
}
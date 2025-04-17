using System.ComponentModel.DataAnnotations;

namespace DreamDayBackend.Models
{
    public class Venue
    {
        [Key]
        public Guid VenueId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // Initialized to empty string

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty; // Initialized to empty string

        [Required]
        public int Capacity { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}
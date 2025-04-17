using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DreamDayBackend.Models;

namespace DreamDayBackend.Models
{
    public class Guest
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string RSVP { get; set; } = string.Empty;
        public string MealPreference { get; set; } = string.Empty;
        public string Seating { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } // EF will handle initialization
    }
}
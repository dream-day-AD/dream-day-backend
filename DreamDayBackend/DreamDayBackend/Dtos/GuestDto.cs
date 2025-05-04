using System.ComponentModel.DataAnnotations;

namespace DreamDayBackend.Dtos
{
    public class GuestDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(20)]
        public string RSVP { get; set; } = "Pending";

        [StringLength(20)]
        public string MealPreference { get; set; } = "None";

        [StringLength(50)]
        public string Seating { get; set; } = string.Empty;
    }
}

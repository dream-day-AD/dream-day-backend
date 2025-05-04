using DreamDayBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Guest
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string RSVP { get; set; } = "Pending";

    [StringLength(20)]
    public string MealPreference { get; set; } = "None";

    [StringLength(50)]
    public string Seating { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }
}

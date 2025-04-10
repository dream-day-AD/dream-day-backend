namespace DreamDayBackend.Models
{
    public class Guest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RSVP { get; set; } = "pending"; // "yes", "no", "maybe", "pending"
        public string MealPreference { get; set; } = "none"; // "vegetarian", "non-vegetarian", "vegan", "none"
        public string Seating { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty; // Foreign key to User
        public User User { get; set; } = null!; // Navigation property
    }
}
namespace DreamDayBackend.Models
{
    public class Guest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RSVP { get; set; } = "pending";
        public string MealPreference { get; set; } = "none";
        public string Seating { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
    }
}
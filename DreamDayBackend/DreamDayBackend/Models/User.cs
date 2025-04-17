using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace DreamDayBackend.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty; // Initialized to empty string
        public string Role { get; set; } = string.Empty; // Initialized to empty string
        public List<Event> Events { get; set; } = new List<Event>();
    }
}
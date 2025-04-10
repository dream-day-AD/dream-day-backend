using Microsoft.AspNetCore.Identity;

namespace DreamDayBackend.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "client"; // Default to client
    }
}
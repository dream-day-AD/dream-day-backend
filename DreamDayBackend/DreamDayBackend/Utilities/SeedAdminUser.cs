using DreamDayBackend.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace DreamDayBackend.Utilities
{
    public static class SeedAdminUser
    {
        public static async System.Threading.Tasks.Task CreateAdminUser(IServiceProvider serviceProvider)
        {
            // Get services for managing users
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Define admin user details
            string adminEmail = "ranil@parlimentjokes.com";
            string adminPassword = "Password123@";
            string adminName = "Ranil Lokka";

            // Check if admin user already exists
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                // Create new admin user
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = adminName,
                    Role = "admin"
                };

                // Add user to database
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    Console.WriteLine("Admin user created successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                Console.WriteLine("Admin user already exists.");
            }
        }
    }
}
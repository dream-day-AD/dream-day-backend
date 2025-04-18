using DreamDayBackend.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace DreamDayBackend.Utilities
{
    public static class SeedPlannerUser
    {
        public static async System.Threading.Tasks.Task CreatePlannerUser(IServiceProvider serviceProvider)
        {
            // Get services for managing users
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Define planner user details
            string plannerEmail = "planner@example.com";
            string plannerPassword = "PlannerPassword123@";
            string plannerName = "Planner User";

            // Check if planner user already exists
            var plannerUser = await userManager.FindByEmailAsync(plannerEmail);
            if (plannerUser == null)
            {
                // Create new planner user
                plannerUser = new ApplicationUser
                {
                    UserName = plannerEmail,
                    Email = plannerEmail,
                    Name = plannerName,
                    Role = "planner"
                };

                // Add user to database
                var result = await userManager.CreateAsync(plannerUser, plannerPassword);
                if (result.Succeeded)
                {
                    Console.WriteLine("Planner user created successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to create planner user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                Console.WriteLine("Planner user already exists.");
            }
        }
    }
}
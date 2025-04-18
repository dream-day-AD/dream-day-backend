using Microsoft.AspNetCore.Mvc;
using DreamDayBackend.Models;
using System.Collections.Generic;

namespace DreamDayBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlannerController : ControllerBase
    {
        private static readonly List<Wedding> MockWeddings = new List<Wedding>
        {
            new Wedding
            {
                Id = "1",
                CoupleName = "John & Jane",
                Date = "2025-06-15",
                Budget = 50000,
                Tasks = new List<DreamDayBackend.Models.Task>
                {
                    new DreamDayBackend.Models.Task { Id = "1", Description = "Book venue", Completed = false },
                    new DreamDayBackend.Models.Task { Id = "2", Description = "Choose menu", Completed = true }
                },
                Vendors = new List<Vendor>
                {
                    new Vendor { Id = "1", Name = "Florist A", Category = "Flowers" },
                    new Vendor { Id = "2", Name = "Caterer B", Category = "Food" }
                }
            },
            new Wedding
            {
                Id = "2",
                CoupleName = "Alice & Bob",
                Date = "2025-07-20",
                Budget = 60000,
                Tasks = new List<DreamDayBackend.Models.Task>
                {
                    new DreamDayBackend.Models.Task { Id = "3", Description = "Select photographer", Completed = false }
                },
                Vendors = new List<Vendor>
                {
                    new Vendor { Id = "3", Name = "DJ C", Category = "Music" }
                }
            },
            new Wedding
            {
                Id = "3",
                CoupleName = "Charlie & Dana",
                Date = "2025-08-25",
                Budget = 55000,
                Tasks = new List<DreamDayBackend.Models.Task>
                {
                    new DreamDayBackend.Models.Task { Id = "4", Description = "Send invitations", Completed = true }
                },
                Vendors = new List<Vendor>
                {
                    new Vendor { Id = "4", Name = "Baker D", Category = "Cake" }
                }
            }
        };

        [HttpGet("weddings")]
        public IActionResult GetWeddings()
        {
            return Ok(MockWeddings);
        }
    }
}
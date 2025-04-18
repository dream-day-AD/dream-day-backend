using System.Numerics;

namespace DreamDayBackend.Models
{
    public class Wedding
    {
        public string Id { get; set; }
        public string CoupleName { get; set; }
        public string Date { get; set; }
        public decimal Budget { get; set; }
        public List<Task> Tasks { get; set; } = new List<Task>();
        public List<Vendor> Vendors { get; set; } = new List<Vendor>();
    }
}
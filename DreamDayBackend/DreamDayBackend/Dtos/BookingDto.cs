using System;
using System.ComponentModel.DataAnnotations;

namespace DreamDayBackend.Dtos
{
    public class BookingDto
    {
        public Guid BookingId { get; set; }

        [Required]
        public Guid EventId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [Required]
        public decimal TotalCost { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResortTralaleritos.Models
{
    public class CheckIn
    {
        [Key]
        public int CheckInId { get; set; }

        [Required]
        [ForeignKey("Guest")]
        public int GuestId { get; set; }

        [Required]
        [ForeignKey("Room")]
        public int RoomId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan CheckInTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime SystemRegisteredDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(255)]
        public string? RegisteredBy { get; set; } // Usuario que registró el check-in

        [StringLength(50)]
        public string? IpAddress { get; set; } // Para auditoría

        // Navigation properties
        public virtual Guest? Guest { get; set; }
        public virtual Room? Room { get; set; }
    }
}

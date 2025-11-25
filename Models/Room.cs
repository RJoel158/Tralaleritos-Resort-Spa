using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResortTralaleritos.Models
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomId { get; set; }

        // Información básica
        [Required]
        [StringLength(10)]
        public string RoomNumber { get; set; } = string.Empty;

        // Relación con RoomType (FK)
        [Required]
        public int RoomTypeId { get; set; }

        public RoomType RoomType { get; set; } = default!;

        [StringLength(300)]
        public string? Description { get; set; }

        // Capacidades
        [Range(1, 10)]
        public int Capacity { get; set; }

        [Range(0, 10)]
        public int Beds { get; set; }

        // Precios
        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerNight { get; set; }

        // Estado / Disponibilidad
        public bool IsAvailable { get; set; } = true;

        public RoomStatus Status { get; set; } = RoomStatus.Available;

        // Servicios Many-to-Many
        public ICollection<Service> Services { get; set; } = new List<Service>();

        // Auditoría
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum RoomStatus
    {
        Available,
        Occupied,
        Cleaning,
        Maintenance,
        Reserved
    }
}

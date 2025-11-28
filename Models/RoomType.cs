using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResortTralaleritos.Models
{
    public class RoomType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty; // Ej: Suite, Deluxe

        [StringLength(300)]
        public string? Description { get; set; }

        // Precio base que heredan las habitaciones
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 20000)]
        public decimal BasePrice { get; set; }

        // Sugerencias para UI / configuración
        [Range(1, 10)]
        public int DefaultCapacity { get; set; }

        [Range(1, 10)]
        public int DefaultBeds { get; set; }

        // Relación 1 -> muchos (RoomType tiene muchas Rooms)
        public ICollection<Room> Rooms { get; set; } = new List<Room>();

        // Servicios recomendados para este tipo (opcional)
        public ICollection<Service>? DefaultServices { get; set; }
    }
}

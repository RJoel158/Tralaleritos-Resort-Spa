using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResortTralaleritos.Models
{
    /// <summary>
    /// Modelo para registrar el histórico de cambios en las habitaciones
    /// Proporciona trazabilidad completa de modificaciones
    /// </summary>
    public class RoomAuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditLogId { get; set; }

        // Relación con la habitación
        [Required]
        [ForeignKey("Room")]
        public int RoomId { get; set; }
        public virtual Room? Room { get; set; }

        // Información del usuario que realizó el cambio
        [Required]
        [StringLength(100)]
        public string ModifiedBy { get; set; } = string.Empty;

        // Fecha y hora de la modificación
        [Required]
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Tipo de operación (Create, Update, Delete)
        [Required]
        [StringLength(50)]
        public string Operation { get; set; } = string.Empty; // "Create", "Update", "Delete"

        // Campo que fue modificado
        [StringLength(100)]
        public string? FieldName { get; set; }

        // Valor anterior
        [StringLength(500)]
        public string? OldValue { get; set; }

        // Valor nuevo
        [StringLength(500)]
        public string? NewValue { get; set; }

        // Descripción general del cambio (para operaciones que afecten múltiples campos)
        [StringLength(1000)]
        public string? ChangeDescription { get; set; }

        // Dirección IP del usuario (para seguridad)
        [StringLength(45)]
        public string? IpAddress { get; set; }
    }
}

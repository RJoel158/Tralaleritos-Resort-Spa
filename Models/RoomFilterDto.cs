using System.ComponentModel.DataAnnotations;

namespace ResortTralaleritos.Models
{
    /// <summary>
    /// DTO para aplicar filtros en la búsqueda de habitaciones
    /// Utilizado en CU-HABITACION-03 para buscar y modificar habitaciones
    /// </summary>
    public class RoomFilterDto
    {
        // Búsqueda por número de habitación
        [StringLength(10)]
        public string? RoomNumber { get; set; }

        // Filtro por tipo de habitación
        [StringLength(50)]
        public string? RoomType { get; set; }

        // Filtro por estado
        public RoomStatus? Status { get; set; }

        // Rango de precio mínimo
        [Range(0, 10000)]
        public decimal? MinPrice { get; set; }

        // Rango de precio máximo
        [Range(0, 10000)]
        public decimal? MaxPrice { get; set; }

        // Filtro por capacidad mínima
        [Range(1, 10)]
        public int? MinCapacity { get; set; }

        // Filtro por disponibilidad
        public bool? IsAvailable { get; set; }

        // Número de página para paginación
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        // Cantidad de registros por página
        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        // Campo para ordenar los resultados
        [StringLength(50)]
        public string? SortBy { get; set; } = "RoomNumber"; // "RoomNumber", "PricePerNight", "Capacity"

        // Dirección del ordenamiento
        public bool SortDescending { get; set; } = false;
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace ResortTralaleritos.Models
{
    public class CheckInSearchDto
    {
        [StringLength(100)]
        public string? SearchTerm { get; set; } // Nombre del huésped o número de reserva

        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        [StringLength(50)]
        public string? SortBy { get; set; } = "RegistrationDate";
        
        public bool SortDescending { get; set; } = true;

        // Filtros adicionales
        [StringLength(50)]
        public string? Status { get; set; } // Pending, CheckedIn, CheckedOut
    }
}

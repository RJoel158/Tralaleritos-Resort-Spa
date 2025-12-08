using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ResortTralaleritos.Models
{
    public class Guest
    {
        [Key]
        public int GuestId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "El correo no es válido")]
        [StringLength(255)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de identificación es requerido")]
        [StringLength(20)]
        public string IdentificationType { get; set; } = string.Empty; // Cédula, Pasaporte, etc.

        [Required(ErrorMessage = "El número de identificación es requerido")]
        [StringLength(50)]
        public string IdentificationNumber { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();

        public string FullName => $"{FirstName} {LastName}";
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResortTralaleritos.Models
{
    public class Service
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "NAME")]
        [StringLength(50, ErrorMessage = "{0} must be: minimum {2} and maximum {1}", MinimumLength = 3)]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s]+$", ErrorMessage = "Only letters and spaces are allowed.")]
        public string? Name { get; set; }

        [StringLength(150, ErrorMessage = "{0} must be less than {1} characters")]
        [Display(Name = "DESCRIPTION")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s]+$", ErrorMessage = "Only letters and spaces are allowed.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Opening time is required")]
        [Display(Name = "OPENING TIME")]
        [DataType(DataType.Time)]
        public DateTime OpeningTime { get; set; }

        [Required(ErrorMessage = "Closing time is required")]
        [Display(Name = "CLOSING TIME")]
        [DataType(DataType.Time)]
        public DateTime ClosingTime { get; set; }

        [Required(ErrorMessage = "Base cost is required")]
        [Display(Name = "BASE COST")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal BaseCost { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "REGISTRATION DATE")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
    }

}

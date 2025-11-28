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

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Name")]
        [StringLength(30, ErrorMessage = "{0} must be: minimum {2} and maximum {1} characters", MinimumLength = 3)]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s]+$", ErrorMessage = "Only letters and spaces are allowed.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(300)]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s]+$", ErrorMessage = "Only letters and spaces are allowed.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Base price is required")]
        [Display(Name = "Base Price")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 20000)]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "Default Capacity is required")]
        [Display(Name = "Default Capacity")]
        [Range(1, 10)]
        public int DefaultCapacity { get; set; }

        [Required(ErrorMessage = "Default beds is required")]
        [Display(Name = "Default Beds")]
        [Range(1, 10)]
        public int DefaultBeds { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        [Display(Name = "Update Date")]
        public DateTime? UpdateDate { get; set; }

        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}

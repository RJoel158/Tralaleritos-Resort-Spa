using System.ComponentModel.DataAnnotations;

namespace ResortTralaleritos.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Name")]
        [StringLength(60, ErrorMessage = "{0} must be: minimum {2} and maximum {1} characters", MinimumLength = 3)]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        [StringLength(30, ErrorMessage = "{0} must be: minimum {2} and maximum {1} characters", MinimumLength = 3)]
        public string? LastName { get; set; }

        [Display(Name = "Second Last Name")]
        [StringLength(30, ErrorMessage = "{0} must be: minimum {2} and maximum {1} characters", MinimumLength = 3)]
        public string? SecondLastName { get; set; }

        [Display(Name = "Password")]
        [StringLength(30, ErrorMessage = "{0} must be: minimum {2} and maximum {1} characters", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        [StringLength(255, ErrorMessage = "{0} must be: minimum {2} and maximum {1} characters", MinimumLength = 3)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public UserStatus Status { get; set; } = UserStatus.Active;

        [DataType(DataType.Date)]
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
    }

    public enum UserStatus
    {
        Active,
        Disabled
    }
}

using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Reporting.Models
{
    public class LoginViewModel
    {
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}

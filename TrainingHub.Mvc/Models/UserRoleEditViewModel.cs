using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Mvc.Models
{
    public class UserRoleEditViewModel
    {
        public int UserId { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string SelectedRole { get; set; } = string.Empty;

        public IReadOnlyList<string> AvailableRoles { get; set; } = Array.Empty<string>();
    }
}
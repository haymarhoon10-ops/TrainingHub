using Microsoft.AspNetCore.Identity;

namespace TrainingHub.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
    }
}
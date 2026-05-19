using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Models
{
    public class Trainee
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = "";

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = "";

        [Phone]
        [StringLength(30)]
        public string PhoneNumber { get; set; } = "";

        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
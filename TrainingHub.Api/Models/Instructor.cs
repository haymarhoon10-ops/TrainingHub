using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Models
{
    public class Instructor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = "";

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = "";

        [Phone]
        [StringLength(30)]
        public string PhoneNumber { get; set; } = "";

        [StringLength(1000)]
        public string Bio { get; set; } = "";

        [StringLength(200)]
        public string ExpertiseArea { get; set; } = "";

        public bool IsAvailable { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<CourseSession> CourseSessions { get; set; } = new List<CourseSession>();

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
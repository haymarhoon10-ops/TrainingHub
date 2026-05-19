using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Models
{
    public class CourseSession
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int InstructorId { get; set; }

        [Required]
        public int ClassroomId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(1, 100)]
        public int Capacity { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Course? Course { get; set; }
        public Instructor? Instructor { get; set; }
        public Classroom? Classroom { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
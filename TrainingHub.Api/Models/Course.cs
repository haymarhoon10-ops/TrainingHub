using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = "";

        [Range(1, 500)]
        public double DurationHours { get; set; }

        [Range(1, 100)]
        public int DefaultCapacity { get; set; }

        [Range(0, 10000)]
        public decimal Fee { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public ICollection<CourseSession> CourseSessions { get; set; } = new List<CourseSession>();
        public ICollection<CertificationTrackCourse> CertificationTrackCourses { get; set; } = new List<CertificationTrackCourse>();
    }
}
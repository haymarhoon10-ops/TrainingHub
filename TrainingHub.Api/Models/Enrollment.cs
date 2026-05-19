using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required]
        public int TraineeId { get; set; }

        [Required]
        public int CourseSessionId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Enrolled";

        public DateTime EnrolledAt { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string AttendanceStatus { get; set; } = "NotStarted";

        [StringLength(50)]
        public string ResultStatus { get; set; } = "Pending";

        public DateTime? ResultRecordedAt { get; set; }

        public Trainee? Trainee { get; set; }
        public CourseSession? CourseSession { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
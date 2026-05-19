using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Models
{
    public class CertificationTrackCourse
    {
        public int Id { get; set; }

        [Required]
        public int CertificationTrackId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public CertificationTrack? CertificationTrack { get; set; }
        public Course? Course { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Models
{
    public class CertificationTrack
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = "";

        [StringLength(1000)]
        public string Description { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public ICollection<CertificationTrackCourse> CertificationTrackCourses { get; set; } = new List<CertificationTrackCourse>();
        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
    }
}
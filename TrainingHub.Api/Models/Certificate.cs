using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Models
{
    public class Certificate
    {
        public int Id { get; set; }

        [Required]
        public int TraineeId { get; set; }

        [Required]
        public int CertificationTrackId { get; set; }

        [Required]
        [StringLength(100)]
        public string CertificateReferenceNumber { get; set; } = "";

        public DateTime IssuedAt { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Eligible";

        public Trainee? Trainee { get; set; }
        public CertificationTrack? CertificationTrack { get; set; }
    }
}
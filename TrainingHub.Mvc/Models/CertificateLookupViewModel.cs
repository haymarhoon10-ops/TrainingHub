using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Mvc.Models
{
    public class CertificateLookupViewModel
    {
        [Required(ErrorMessage = "Enter a trainee ID.")]
        [Range(1, int.MaxValue, ErrorMessage = "Enter a valid trainee ID.")]
        [Display(Name = "Trainee ID")]
        public int? TraineeId { get; set; }

        [Required(ErrorMessage = "Enter a certificate reference number.")]
        [Display(Name = "Certificate reference number")]
        [StringLength(100, ErrorMessage = "Certificate reference number must not exceed 100 characters.")]
        [RegularExpression("^[A-Za-z0-9-]+$", ErrorMessage = "Use only letters, numbers, and hyphens.")]
        public string? ReferenceNumber { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class PublicCertificateDetailsViewModel
    {
        public string CertificateReferenceNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TraineeName { get; set; } = string.Empty;
        public string CertificationTrack { get; set; } = string.Empty;
        public string TrackDescription { get; set; } = string.Empty;
        public string IssueDate { get; set; } = string.Empty;
        public List<string> CompletedCourses { get; set; } = new();
    }
}

using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Mvc.Models
{
    public class CertificateLookupViewModel
    {
        [Display(Name = "Certificate reference number")]
        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        public string? ErrorMessage { get; set; }
    }
}

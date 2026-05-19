using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int TraineeId { get; set; }

        [Required]
        public int EnrollmentId { get; set; }

        [Range(0.01, 10000)]
        public decimal AmountPaid { get; set; }

        public DateTime PaidAt { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "";

        [StringLength(100)]
        public string ReferenceNumber { get; set; } = "";

        [StringLength(500)]
        public string Notes { get; set; } = "";

        public Trainee? Trainee { get; set; }
        public Enrollment? Enrollment { get; set; }
    }
}
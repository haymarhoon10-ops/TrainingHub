using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Reporting.Models
{
    public class EnrollmentStatDto
    {
        [Required]
        public required string CourseName { get; set; }
        [Required]
        public required string CategoryName { get; set; }
        public int TotalCapacity { get; set; }
        public int TotalEnrolled { get; set; }

        // We can even calculate helpful extra fields!
        public int RemainingSpots => TotalCapacity - TotalEnrolled;
        public double FillPercentage => TotalCapacity == 0 ? 0 : Math.Round(((double)TotalEnrolled / TotalCapacity) * 100, 2);
    }
}
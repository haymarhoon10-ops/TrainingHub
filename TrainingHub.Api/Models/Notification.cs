using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "General";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        public int? TraineeId { get; set; }
        public int? InstructorId { get; set; }
        [StringLength(500)]
        public string? Link { get; set; } = "";


        public Trainee? Trainee { get; set; }
        public Instructor? Instructor { get; set; }
    }
}
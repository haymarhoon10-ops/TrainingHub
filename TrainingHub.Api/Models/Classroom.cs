using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Models
{
    public class Classroom
    {
        public int Id { get; set; }

        [Range(1, 9999)]
        public int RoomCode { get; set; }

        [Range(1, 200)]
        public int Capacity { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = "";

        public bool HasProjector { get; set; }
        public bool HasLabComputer { get; set; }
        public bool IsAvailable { get; set; } = true;

        public ICollection<CourseSession> CourseSessions { get; set; } = new List<CourseSession>();
    }
}
using System.ComponentModel.DataAnnotations;

namespace TrainingHub.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";
        [StringLength(500)]
        public string Description { get; set; } = "";

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
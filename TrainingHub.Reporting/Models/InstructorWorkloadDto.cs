namespace TrainingHub.Reporting.Models
{
    public class InstructorWorkloadDto
    {
        public string InstructorName { get; set; }
        public string Department { get; set; }
        public int ActiveCourses { get; set; }
        public int TotalTrainees { get; set; }
    }
}
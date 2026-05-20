namespace TrainingHub.Mvc.Models
{
    public class CertificationProgressViewModel
    {
        public string TraineeName { get; set; } = "";
        public string CertificationTrackName { get; set; } = "";
        public int CompletedCourses { get; set; }
        public int TotalRequiredCourses { get; set; }
        public decimal ProgressPercentage { get; set; }
    }
}

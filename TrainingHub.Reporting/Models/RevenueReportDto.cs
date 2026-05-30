namespace TrainingHub.Reporting.Models
{
    public class RevenueReportDto
    {
        public string CourseName { get; set; } = string.Empty;
        public decimal TotalExpectedRevenue { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalOutstanding { get; set; }
    }
}

namespace TrainingHub.Api.Models
{
    // Represents the outgoing data structure for enrollment statistics
    public sealed record EnrollmentStatResponse(
        string CourseName,
        string CategoryName,
        int TotalCapacity,
        int TotalEnrolled);
}
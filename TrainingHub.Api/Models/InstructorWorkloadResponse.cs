namespace TrainingHub.Api.Models
{
    public sealed record InstructorWorkloadResponse(
        string InstructorName,
        string Department,
        int ActiveCourses,
        int TotalTrainees);
}
namespace TrainingHub.Api.DTOs
{
    /// <summary>
    /// Standard error response for API endpoints
    /// </summary>
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string[]>? Errors { get; set; }
    }

    /// <summary>
    /// Certificate response DTO for API responses
    /// </summary>
    public class CertificateResponse
    {
        public int CertificateId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string TraineeName { get; set; } = string.Empty;
        public string TrackName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
    }

    /// <summary>
    /// Certificate lookup response
    /// </summary>
    public class CertificateLookupResponse
    {
        public string CertificateReferenceNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TraineeName { get; set; } = string.Empty;
        public string CertificationTrack { get; set; } = string.Empty;
        public string TrackDescription { get; set; } = string.Empty;
        public string IssueDate { get; set; } = string.Empty;
        public List<string> CompletedCourses { get; set; } = new();
    }

    /// <summary>
    /// Course response DTO
    /// </summary>
    public class CourseResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double DurationHours { get; set; }
        public int DefaultCapacity { get; set; }
        public decimal Fee { get; set; }
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? PrerequisiteCourseId { get; set; }
        public string? PrerequisiteCourseName { get; set; }
    }

    /// <summary>
    /// Course session response DTO
    /// </summary>
    public class CourseSessionResponse
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int ClassroomId { get; set; }
        public string ClassroomName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxEnrollment { get; set; }
        public int CurrentEnrollment { get; set; }
    }

    /// <summary>
    /// Enrollment response DTO
    /// </summary>
    public class EnrollmentResponse
    {
        public int Id { get; set; }
        public int TraineeId { get; set; }
        public string TraineeName { get; set; } = string.Empty;
        public int CourseSessionId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
        public string AttendanceStatus { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ResultStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Authentication response DTO
    /// </summary>
    public class AuthMeResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}

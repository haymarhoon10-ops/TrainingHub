namespace TrainingHub.Mvc.Realtime;

public class EnrollmentCounterUpdate
{
    public int CourseSessionId { get; init; }

    public int? EnrollmentId { get; init; }

    public int Capacity { get; init; }

    public int CurrentEnrollments { get; init; }

    public int RemainingSpots { get; init; }

    public string ChangeType { get; init; } = "updated";

    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

namespace TrainingHub.Mvc.Realtime;

public class NotificationRealtimePayload
{
    public int NotificationId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public bool IsRead { get; init; }

    public int? TraineeId { get; init; }

    public int? InstructorId { get; init; }

    public string TraineeDisplay { get; init; } = string.Empty;

    public string InstructorDisplay { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

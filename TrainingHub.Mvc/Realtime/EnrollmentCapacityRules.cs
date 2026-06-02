namespace TrainingHub.Mvc.Realtime;

public static class EnrollmentCapacityRules
{
    public const string DroppedStatus = "Dropped";

    public static bool CountsTowardCapacity(string? status)
    {
        return !string.Equals(status, DroppedStatus, StringComparison.OrdinalIgnoreCase);
    }
}

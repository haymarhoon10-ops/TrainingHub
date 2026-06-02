using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TrainingHub.Security;

namespace TrainingHub.Mvc.Hubs;

[Authorize(Roles = RoleNames.TrainingCoordinator + "," + RoleNames.Instructor + "," + RoleNames.Trainee)]
public class EnrollmentRealtimeHub : Hub
{
    public const string HubRoute = "/hubs/enrollments";
    private const string CoordinatorNotificationsGroupName = "notifications-coordinators";

    public static string GetCourseSessionGroupName(int courseSessionId) => $"course-session-{courseSessionId}";

    public static string GetCoordinatorNotificationsGroupName() => CoordinatorNotificationsGroupName;

    public static string GetTraineeNotificationsGroupName(string email) => $"notifications-trainee-{NormalizeGroupKey(email)}";

    public static string GetInstructorNotificationsGroupName(string email) => $"notifications-instructor-{NormalizeGroupKey(email)}";

    public Task SubscribeToCourseSessionAsync(int courseSessionId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GetCourseSessionGroupName(courseSessionId));
    }

    public Task UnsubscribeFromCourseSessionAsync(int courseSessionId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetCourseSessionGroupName(courseSessionId));
    }

    public Task SubscribeToCurrentUserNotificationsAsync()
    {
        var email = Context.User?.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email))
        {
            return Task.CompletedTask;
        }

        var subscriptions = new List<Task>();

        if (Context.User?.IsInRole(RoleNames.TrainingCoordinator) == true)
        {
            subscriptions.Add(Groups.AddToGroupAsync(Context.ConnectionId, GetCoordinatorNotificationsGroupName()));
        }

        if (Context.User?.IsInRole(RoleNames.Trainee) == true)
        {
            subscriptions.Add(Groups.AddToGroupAsync(Context.ConnectionId, GetTraineeNotificationsGroupName(email)));
        }

        if (Context.User?.IsInRole(RoleNames.Instructor) == true)
        {
            subscriptions.Add(Groups.AddToGroupAsync(Context.ConnectionId, GetInstructorNotificationsGroupName(email)));
        }

        return Task.WhenAll(subscriptions);
    }

    public Task UnsubscribeFromCurrentUserNotificationsAsync()
    {
        var email = Context.User?.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email))
        {
            return Task.CompletedTask;
        }

        var subscriptions = new List<Task>();

        if (Context.User?.IsInRole(RoleNames.TrainingCoordinator) == true)
        {
            subscriptions.Add(Groups.RemoveFromGroupAsync(Context.ConnectionId, GetCoordinatorNotificationsGroupName()));
        }

        if (Context.User?.IsInRole(RoleNames.Trainee) == true)
        {
            subscriptions.Add(Groups.RemoveFromGroupAsync(Context.ConnectionId, GetTraineeNotificationsGroupName(email)));
        }

        if (Context.User?.IsInRole(RoleNames.Instructor) == true)
        {
            subscriptions.Add(Groups.RemoveFromGroupAsync(Context.ConnectionId, GetInstructorNotificationsGroupName(email)));
        }

        return Task.WhenAll(subscriptions);
    }

    private static string NormalizeGroupKey(string value)
    {
        return value.Trim().ToLowerInvariant();
    }
}

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Data;
using TrainingHub.Models;
using TrainingHub.Mvc.Hubs;
using TrainingHub.Security;

namespace TrainingHub.Mvc.Services;

public class SignalRRealtimeNotifier : IRealtimeNotifier
{
    private readonly TrainingHubDbContext _context;
    private readonly IHubContext<EnrollmentRealtimeHub> _hubContext;

    public SignalRRealtimeNotifier(TrainingHubDbContext context, IHubContext<EnrollmentRealtimeHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task PublishEnrollmentCreatedAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        var payload = await BuildEnrollmentCounterUpdateAsync(
            enrollment.CourseSessionId,
            enrollment.Id,
            "created",
            cancellationToken);

        await BroadcastEnrollmentCounterUpdateAsync(payload, cancellationToken);
    }

    public async Task PublishEnrollmentUpdatedAsync(
        Enrollment enrollment,
        int? previousCourseSessionId = null,
        CancellationToken cancellationToken = default)
    {
        if (previousCourseSessionId.HasValue && previousCourseSessionId.Value != enrollment.CourseSessionId)
        {
            var previousPayload = await BuildEnrollmentCounterUpdateAsync(
                previousCourseSessionId.Value,
                enrollment.Id,
                "moved-from",
                cancellationToken);

            await BroadcastEnrollmentCounterUpdateAsync(previousPayload, cancellationToken);
        }

        var currentPayload = await BuildEnrollmentCounterUpdateAsync(
            enrollment.CourseSessionId,
            enrollment.Id,
            "updated",
            cancellationToken);

        await BroadcastEnrollmentCounterUpdateAsync(currentPayload, cancellationToken);
    }

    public async Task PublishEnrollmentDeletedAsync(int courseSessionId, int enrollmentId, CancellationToken cancellationToken = default)
    {
        var payload = await BuildEnrollmentCounterUpdateAsync(
            courseSessionId,
            enrollmentId,
            "deleted",
            cancellationToken);

        await BroadcastEnrollmentCounterUpdateAsync(payload, cancellationToken);
    }

    public async Task PublishNotificationCreatedAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        var traineeQuery = notification.TraineeId.HasValue
            ? await _context.Trainees
                .AsNoTracking()
                .Where(trainee => trainee.Id == notification.TraineeId.Value)
                .Select(trainee => new { trainee.Email, trainee.FullName })
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var instructorQuery = notification.InstructorId.HasValue
            ? await _context.Instructors
                .AsNoTracking()
                .Where(instructor => instructor.Id == notification.InstructorId.Value)
                .Select(instructor => new { instructor.Email, instructor.Name })
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var payload = new NotificationRealtimePayload
        {
            NotificationId = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            IsRead = notification.IsRead,
            TraineeId = notification.TraineeId,
            InstructorId = notification.InstructorId,
            TraineeDisplay = traineeQuery?.Email ?? string.Empty,
            InstructorDisplay = instructorQuery?.Name ?? string.Empty,
            CreatedAt = notification.CreatedAt
        };

        await BroadcastNotificationCreatedAsync(
            payload,
            traineeQuery?.Email,
            instructorQuery?.Email,
            cancellationToken);
    }

    private async Task<EnrollmentCounterUpdate> BuildEnrollmentCounterUpdateAsync(
        int courseSessionId,
        int? enrollmentId,
        string changeType,
        CancellationToken cancellationToken)
    {
        var sessionSnapshot = await _context.CourseSessions
            .AsNoTracking()
            .Where(courseSession => courseSession.Id == courseSessionId)
            .Select(courseSession => new
            {
                courseSession.Id,
                courseSession.Capacity
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (sessionSnapshot == null)
        {
            return new EnrollmentCounterUpdate
            {
                CourseSessionId = courseSessionId,
                EnrollmentId = enrollmentId,
                ChangeType = changeType
            };
        }

        var currentEnrollments = await _context.Enrollments
            .AsNoTracking()
            .CountAsync(
                enrollment => enrollment.CourseSessionId == courseSessionId &&
                    EnrollmentCapacityRules.CountsTowardCapacity(enrollment.Status),
                cancellationToken);

        return new EnrollmentCounterUpdate
        {
            CourseSessionId = courseSessionId,
            EnrollmentId = enrollmentId,
            Capacity = sessionSnapshot.Capacity,
            CurrentEnrollments = currentEnrollments,
            RemainingSpots = Math.Max(sessionSnapshot.Capacity - currentEnrollments, 0),
            ChangeType = changeType,
            OccurredAt = DateTime.UtcNow
        };
    }

    private Task BroadcastEnrollmentCounterUpdateAsync(
        EnrollmentCounterUpdate payload,
        CancellationToken cancellationToken)
    {
        return _hubContext.Clients
            .Group(EnrollmentRealtimeHub.GetCourseSessionGroupName(payload.CourseSessionId))
            .SendAsync("EnrollmentCounterUpdated", payload, cancellationToken);
    }

    private Task BroadcastNotificationCreatedAsync(
        NotificationRealtimePayload payload,
        string? traineeEmail,
        string? instructorEmail,
        CancellationToken cancellationToken)
    {
        var recipientGroups = new List<string>
        {
            EnrollmentRealtimeHub.GetCoordinatorNotificationsGroupName()
        };

        if (!string.IsNullOrWhiteSpace(traineeEmail))
        {
            recipientGroups.Add(EnrollmentRealtimeHub.GetTraineeNotificationsGroupName(traineeEmail));
        }

        if (!string.IsNullOrWhiteSpace(instructorEmail))
        {
            recipientGroups.Add(EnrollmentRealtimeHub.GetInstructorNotificationsGroupName(instructorEmail));
        }

        return _hubContext.Clients
            .Groups(recipientGroups.Distinct())
            .SendAsync("NotificationCreated", payload, cancellationToken);
    }
}

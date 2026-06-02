using TrainingHub.Models;

namespace TrainingHub.Mvc.Services;

public interface IRealtimeNotifier
{
    Task PublishEnrollmentCreatedAsync(Enrollment enrollment, CancellationToken cancellationToken = default);

    Task PublishEnrollmentUpdatedAsync(Enrollment enrollment, int? previousCourseSessionId = null, CancellationToken cancellationToken = default);

    Task PublishEnrollmentDeletedAsync(int courseSessionId, int enrollmentId, CancellationToken cancellationToken = default);

    Task PublishNotificationCreatedAsync(Notification notification, CancellationToken cancellationToken = default);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Data;
using TrainingHub.Models;
using TrainingHub.Mvc.Realtime;
using TrainingHub.Mvc.Services;
using TrainingHub.Security;

namespace TrainingHub.Mvc.Controllers
{
    [Authorize(Roles = RoleNames.TrainingCoordinator + "," + RoleNames.Instructor + "," + RoleNames.Trainee)]
    public class EnrollmentsController : Controller
    {
        private static readonly HashSet<string> AllowedEnrollmentStatuses = new(StringComparer.Ordinal)
        {
            "Enrolled",
            "Confirmed",
            "Attending",
            "Completed",
            "Dropped"
        };

        private static readonly HashSet<string> AllowedAttendanceStatuses = new(StringComparer.Ordinal)
        {
            "Pending",
            "Present",
            "Absent"
        };

        private static readonly HashSet<string> AllowedResultStatuses = new(StringComparer.Ordinal)
        {
            "Pending",
            "Pass",
            "Fail"
        };

        private readonly TrainingHubDbContext _context;
        private readonly IRealtimeNotifier _realtimeNotifier;

        public EnrollmentsController(TrainingHubDbContext context, IRealtimeNotifier realtimeNotifier)
        {
            _context = context;
            _realtimeNotifier = realtimeNotifier;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index()
        {
            var currentEmail = User.Identity?.Name;
            IQueryable<Enrollment> trainingHubDbContext = _context.Enrollments
                .Include(e => e.CourseSession)
                    .ThenInclude(cs => cs.Course)
                .Include(e => e.CourseSession)
                    .ThenInclude(cs => cs.Instructor)
                .Include(e => e.Trainee);

            if (User.IsInRole(RoleNames.Instructor) && !User.IsInRole(RoleNames.TrainingCoordinator))
            {
                trainingHubDbContext = trainingHubDbContext.Where(e => e.CourseSession != null && e.CourseSession.Instructor != null && e.CourseSession.Instructor.Email == currentEmail);
            }
            else if (User.IsInRole(RoleNames.Trainee) && !User.IsInRole(RoleNames.TrainingCoordinator))
            {
                trainingHubDbContext = trainingHubDbContext.Where(e => e.Trainee != null && e.Trainee.Email == currentEmail);
            }

            return View(await trainingHubDbContext.ToListAsync());
        }

        // GET: Enrollments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var enrollment = await _context.Enrollments
                .Include(e => e.CourseSession)
                    .ThenInclude(cs => cs.Course)
                .Include(e => e.CourseSession)
                    .ThenInclude(cs => cs.Instructor)
                .Include(e => e.Trainee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (enrollment == null)
                return NotFound();

            if (!User.IsInRole(RoleNames.TrainingCoordinator))
            {
                var currentEmail = User.Identity?.Name;
                var allowed = User.IsInRole(RoleNames.Instructor)
                    ? string.Equals(enrollment.CourseSession?.Instructor?.Email, currentEmail, StringComparison.OrdinalIgnoreCase)
                    : User.IsInRole(RoleNames.Trainee)
                        ? string.Equals(enrollment.Trainee?.Email, currentEmail, StringComparison.OrdinalIgnoreCase)
                        : false;

                if (!allowed)
                {
                    return Forbid();
                }
            }

            return View(enrollment);
        }

        // GET: Enrollments/Create
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: Enrollments/Create
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TraineeId,CourseSessionId,Status,EnrolledAt,AttendanceStatus,ResultStatus,ResultRecordedAt")] Enrollment enrollment)
        {
            ValidateEnrollmentLifecycleValues(enrollment);
            await ValidateEnrollmentRules(enrollment);

            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                await _realtimeNotifier.PublishEnrollmentCreatedAsync(enrollment, HttpContext.RequestAborted);
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(enrollment);
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        [Authorize(Roles = RoleNames.TrainingCoordinator + "," + RoleNames.Instructor)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var enrollment = await _context.Enrollments
                .Include(e => e.CourseSession)
                    .ThenInclude(cs => cs.Instructor)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrollment == null)
                return NotFound();

            if (User.IsInRole(RoleNames.Instructor) && !User.IsInRole(RoleNames.TrainingCoordinator))
            {
                var currentEmail = User.Identity?.Name;
                if (!string.Equals(enrollment.CourseSession?.Instructor?.Email, currentEmail, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }

                ViewBag.IsAssessmentOnly = true;
            }

            PopulateDropdowns(enrollment);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        [Authorize(Roles = RoleNames.TrainingCoordinator + "," + RoleNames.Instructor)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TraineeId,CourseSessionId,Status,EnrolledAt,AttendanceStatus,ResultStatus,ResultRecordedAt")] Enrollment enrollment)
        {
            if (id != enrollment.Id)
                return NotFound();

            if (User.IsInRole(RoleNames.Instructor) && !User.IsInRole(RoleNames.TrainingCoordinator))
            {
                var instructorEnrollment = await _context.Enrollments
                    .Include(e => e.CourseSession)
                        .ThenInclude(cs => cs.Instructor)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (instructorEnrollment == null)
                {
                    return NotFound();
                }

                var currentEmail = User.Identity?.Name;
                if (!string.Equals(instructorEnrollment.CourseSession?.Instructor?.Email, currentEmail, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }

                if (instructorEnrollment.CourseSession != null && instructorEnrollment.CourseSession.EndDate > DateTime.UtcNow)
                {
                    ModelState.AddModelError(string.Empty, "Assessment results can only be recorded after the session ends.");
                }

                instructorEnrollment.AttendanceStatus = enrollment.AttendanceStatus;
                instructorEnrollment.ResultStatus = enrollment.ResultStatus;
                instructorEnrollment.ResultRecordedAt = string.Equals(enrollment.ResultStatus, "Pending", StringComparison.Ordinal)
                    ? null
                    : enrollment.ResultRecordedAt ?? DateTime.UtcNow;

                if (!string.Equals(instructorEnrollment.Status, "Dropped", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(instructorEnrollment.ResultStatus, "Pending", StringComparison.Ordinal))
                {
                    instructorEnrollment.Status = "Completed";
                }

                ValidateEnrollmentLifecycleValues(instructorEnrollment);

                if (!ModelState.IsValid)
                {
                    ViewBag.IsAssessmentOnly = true;
                    PopulateDropdowns(instructorEnrollment);
                    return View(instructorEnrollment);
                }

                Notification? resultNotification = null;
                if (!string.Equals(instructorEnrollment.ResultStatus, "Pending", StringComparison.Ordinal))
                {
                    resultNotification = new Notification
                    {
                        Title = "Assessment Result Recorded",
                        Message = $"Your result for session {instructorEnrollment.CourseSessionId} was recorded as {instructorEnrollment.ResultStatus}.",
                        Type = "Assessment",
                        TraineeId = instructorEnrollment.TraineeId,
                        CreatedAt = DateTime.Now
                    };

                    _context.Notifications.Add(resultNotification);
                }

                await _context.SaveChangesAsync();

                if (resultNotification != null)
                {
                    await _realtimeNotifier.PublishNotificationCreatedAsync(resultNotification, HttpContext.RequestAborted);
                }

                return RedirectToAction(nameof(Index));
            }

            ValidateEnrollmentLifecycleValues(enrollment);
            await ValidateEnrollmentRules(enrollment);
            var existingEnrollment = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new { e.CourseSessionId })
                .FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
                        return NotFound();
                    else
                        throw;
                }

                await _realtimeNotifier.PublishEnrollmentUpdatedAsync(
                    enrollment,
                    existingEnrollment?.CourseSessionId,
                    HttpContext.RequestAborted);
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(enrollment);
            return View(enrollment);
        }

        [Authorize(Roles = RoleNames.Trainee)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollInSession(int courseSessionId)
        {
            var currentEmail = User.Identity?.Name;
            var trainee = await _context.Trainees.FirstOrDefaultAsync(t => t.Email == currentEmail);

            if (trainee == null)
            {
                TempData["EnrollmentError"] = "No trainee profile is linked to your account.";
                return RedirectToAction("Details", "CourseSessions", new { id = courseSessionId });
            }

            var enrollment = new Enrollment
            {
                TraineeId = trainee.Id,
                CourseSessionId = courseSessionId,
                Status = "Enrolled",
                EnrolledAt = DateTime.Now,
                AttendanceStatus = "Pending",
                ResultStatus = "Pending"
            };

            ValidateEnrollmentLifecycleValues(enrollment);
            await ValidateEnrollmentRules(enrollment);

            if (!ModelState.IsValid)
            {
                TempData["EnrollmentError"] = string.Join(" ", ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage)
                    .Where(message => !string.IsNullOrWhiteSpace(message)));

                return RedirectToAction("Details", "CourseSessions", new { id = courseSessionId });
            }

            _context.Enrollments.Add(enrollment);

            var notification = new Notification
            {
                Title = "Enrollment Confirmed",
                Message = $"You have been enrolled in session {courseSessionId}.",
                Type = "Enrollment",
                TraineeId = trainee.Id,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();
            await _realtimeNotifier.PublishEnrollmentCreatedAsync(enrollment, HttpContext.RequestAborted);
            await _realtimeNotifier.PublishNotificationCreatedAsync(notification, HttpContext.RequestAborted);

            TempData["EnrollmentSuccess"] = "You have been enrolled successfully.";
            return RedirectToAction("Details", "CourseSessions", new { id = courseSessionId });
        }

        // GET: Enrollments/Delete/5
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var enrollment = await _context.Enrollments
                .Include(e => e.CourseSession)
                .Include(e => e.Trainee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (enrollment == null)
                return NotFound();

            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);

            if (enrollment == null)
                return NotFound();
            // If there are payments, mark for refund/credit instead of deleting
            var payments = await _context.Payments.Where(p => p.EnrollmentId == enrollment.Id).ToListAsync();
            if (payments.Any())
            {
                // Mark enrollment as Dropped and create a refund record via Notification for coordinator to process
                enrollment.Status = "Dropped";
                _context.Update(enrollment);

                var totalPaid = payments.Sum(p => p.AmountPaid);
                var notification = new Notification
                {
                    Title = "Enrollment Dropped - Refund Required",
                    Message = $"Enrollment {enrollment.Id} was dropped. Total paid: {totalPaid:C}. Please process refund or credit.",
                    Type = "Finance",
                    TraineeId = enrollment.TraineeId,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();
                await _realtimeNotifier.PublishEnrollmentUpdatedAsync(enrollment, enrollment.CourseSessionId, HttpContext.RequestAborted);
                await _realtimeNotifier.PublishNotificationCreatedAsync(notification, HttpContext.RequestAborted);
                return RedirectToAction(nameof(Index));
            }

            var courseSessionId = enrollment.CourseSessionId;
            var enrollmentId = enrollment.Id;

            try
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
                await _realtimeNotifier.PublishEnrollmentDeletedAsync(courseSessionId, enrollmentId, HttpContext.RequestAborted);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete enrollment because related records exist.");
                return View("Delete", enrollment);
            }
        }

        private async Task ValidateEnrollmentRules(Enrollment enrollment)
        {
            var duplicateEnrollment = await _context.Enrollments.AnyAsync(e =>
                e.TraineeId == enrollment.TraineeId &&
                e.CourseSessionId == enrollment.CourseSessionId &&
                e.Id != enrollment.Id);

            if (duplicateEnrollment)
            {
                ModelState.AddModelError("", "This trainee is already enrolled in this session.");
            }

            var session = await _context.CourseSessions
                .Include(cs => cs.Enrollments)
                .FirstOrDefaultAsync(cs => cs.Id == enrollment.CourseSessionId);

            if (session == null)
            {
                ModelState.AddModelError("", "Selected course session does not exist.");
                return;
            }

            if (session.Enrollments.Count(e =>
                    e.Id != enrollment.Id &&
                    EnrollmentCapacityRules.CountsTowardCapacity(e.Status)) >= session.Capacity &&
                EnrollmentCapacityRules.CountsTowardCapacity(enrollment.Status))
            {
                ModelState.AddModelError("", "This session is already full.");
            }

            var selectedSession = await _context.CourseSessions
                .Include(cs => cs.Course)
                .FirstOrDefaultAsync(cs => cs.Id == enrollment.CourseSessionId);

            if (selectedSession?.Course?.PrerequisiteCourseId != null)
            {
                var prerequisiteCompleted = await _context.Enrollments
                    .Include(e => e.CourseSession)
                    .AnyAsync(e =>
                        e.TraineeId == enrollment.TraineeId &&
                        e.CourseSession!.CourseId == selectedSession.Course.PrerequisiteCourseId &&
                        e.Status == "Completed" &&
                        e.ResultStatus == "Pass");

                if (!prerequisiteCompleted)
                {
                    ModelState.AddModelError(
                        "",
                        "Trainee has not completed the prerequisite course.");
                }
            }
        }

        private void ValidateEnrollmentLifecycleValues(Enrollment enrollment)
        {
            if (!AllowedEnrollmentStatuses.Contains(enrollment.Status))
            {
                ModelState.AddModelError(nameof(Enrollment.Status), "Select a valid enrollment status.");
            }

            if (!AllowedAttendanceStatuses.Contains(enrollment.AttendanceStatus))
            {
                ModelState.AddModelError(nameof(Enrollment.AttendanceStatus), "Select a valid attendance status.");
            }

            if (!AllowedResultStatuses.Contains(enrollment.ResultStatus))
            {
                ModelState.AddModelError(nameof(Enrollment.ResultStatus), "Select a valid result status.");
            }
        }

        private void PopulateDropdowns(Enrollment? enrollment = null)
        {
            var sessions = _context.CourseSessions
                .Include(cs => cs.Course)
                .ToList();

            var sessionList = sessions.Select(cs => new
            {
                Id = cs.Id,
                Name = $"Session {cs.Id} - {cs.Course?.Title}"
            });

            ViewData["CourseSessionId"] = new SelectList(
                sessionList,
                "Id",
                "Name",
                enrollment?.CourseSessionId);

            ViewData["TraineeId"] = new SelectList(
                _context.Trainees,
                "Id",
                "Email",
                enrollment?.TraineeId);
        }

        private bool EnrollmentExists(int id)
        {
            return _context.Enrollments.Any(e => e.Id == id);
        }
    }
}

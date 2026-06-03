using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Data;
using TrainingHub.Models;
using TrainingHub.Security;

namespace TrainingHub.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
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

        private readonly TrainingHubDbContext _dbContext;

        public EnrollmentsController(TrainingHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Authorize(Roles = "TrainingCoordinator,Instructor")]
        public async Task<IActionResult> GetAll()
        {
            var enrollments = await GetEnrollmentQuery()
                .OrderByDescending(enrollment => enrollment.EnrolledAt)
                .ToListAsync();

            return Ok(enrollments.Select(BuildEnrollmentResponse));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var enrollment = await GetEnrollmentQuery()
                .FirstOrDefaultAsync(entity => entity.Id == id);

            if (enrollment == null)
            {
                return NotFound();
            }

            if (User.IsInRole(RoleNames.Trainee))
            {
                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (!string.Equals(userEmail, enrollment.Trainee?.Email, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }
            }
            else if (User.IsInRole(RoleNames.Instructor))
            {
                var currentInstructor = await GetCurrentInstructorAsync();
                if (currentInstructor == null || enrollment.CourseSession == null || currentInstructor.Id != enrollment.CourseSession.InstructorId)
                {
                    return Forbid();
                }
            }

            return Ok(BuildEnrollmentResponse(enrollment));
        }

        [HttpPost]
        [Authorize(Roles = "TrainingCoordinator,Instructor,Trainee")]
        public async Task<IActionResult> Create([FromBody] Enrollment request)
        {
            if (!await ReferencesExistAsync(request.TraineeId, request.CourseSessionId))
            {
                return BadRequest(new { message = "The specified trainee or course session does not exist." });
            }

            if (User.IsInRole("Trainee"))
            {
                var currentTrainee = await GetCurrentTraineeAsync();
                if (currentTrainee == null || currentTrainee.Id != request.TraineeId)
                {
                    return Forbid();
                }

                request.Status = "Enrolled";
                request.AttendanceStatus = "Pending";
                request.ResultStatus = "Pending";
                request.ResultRecordedAt = null;
            }

            ValidateEnrollmentLifecycleValues(request);
            await ValidateEnrollmentBusinessRulesAsync(request);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var enrollment = new Enrollment
            {
                TraineeId = request.TraineeId,
                CourseSessionId = request.CourseSessionId,
                Status = request.Status,
                AttendanceStatus = request.AttendanceStatus,
                ResultStatus = request.ResultStatus,
                ResultRecordedAt = request.ResultRecordedAt
            };

            _dbContext.Enrollments.Add(enrollment);
            await _dbContext.SaveChangesAsync();

            var createdEnrollment = await GetEnrollmentQuery()
                .FirstAsync(entity => entity.Id == enrollment.Id);

            return CreatedAtAction(nameof(GetById), new { id = createdEnrollment.Id }, BuildEnrollmentResponse(createdEnrollment));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "TrainingCoordinator,Instructor")]
        public async Task<IActionResult> Update(int id, [FromBody] Enrollment request)
        {
            var enrollment = await _dbContext.Enrollments
                .Include(e => e.CourseSession)
                .FirstOrDefaultAsync(entity => entity.Id == id);

            if (enrollment == null)
            {
                return NotFound();
            }

            Instructor? currentInstructor = null;
            if (User.IsInRole(RoleNames.Instructor))
            {
                currentInstructor = await GetCurrentInstructorAsync();
                if (currentInstructor == null || enrollment.CourseSession == null || currentInstructor.Id != enrollment.CourseSession.InstructorId)
                {
                    return Forbid();
                }
            }

            if (!await ReferencesExistAsync(request.TraineeId, request.CourseSessionId))
            {
                return BadRequest(new { message = "The specified trainee or course session does not exist." });
            }

            if (currentInstructor != null)
            {
                var canAccessTargetSession = await _dbContext.CourseSessions
                    .AnyAsync(cs => cs.Id == request.CourseSessionId && cs.InstructorId == currentInstructor.Id);

                if (!canAccessTargetSession)
                {
                    return Forbid();
                }
            }

            ValidateEnrollmentLifecycleValues(request);
            request.Id = id;
            await ValidateEnrollmentBusinessRulesAsync(request);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            enrollment.TraineeId = request.TraineeId;
            enrollment.CourseSessionId = request.CourseSessionId;
            enrollment.Status = request.Status;
            enrollment.AttendanceStatus = request.AttendanceStatus;
            enrollment.ResultStatus = request.ResultStatus;
            enrollment.ResultRecordedAt = request.ResultRecordedAt;

            await _dbContext.SaveChangesAsync();

            var updatedEnrollment = await GetEnrollmentQuery()
                .FirstAsync(entity => entity.Id == id);

            return Ok(BuildEnrollmentResponse(updatedEnrollment));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "TrainingCoordinator")]
        public async Task<IActionResult> Delete(int id)
        {
            var enrollment = await _dbContext.Enrollments.FirstOrDefaultAsync(entity => entity.Id == id);

            if (enrollment == null)
            {
                return NotFound();
            }

            _dbContext.Enrollments.Remove(enrollment);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private IQueryable<Enrollment> GetEnrollmentQuery()
        {
            return _dbContext.Enrollments
                .AsNoTracking()
                .Include(enrollment => enrollment.Trainee)
                .Include(enrollment => enrollment.CourseSession!)
                    .ThenInclude(courseSession => courseSession.Course);
        }

        private async Task<bool> ReferencesExistAsync(int traineeId, int courseSessionId)
        {
            var traineeExists = await _dbContext.Trainees.AnyAsync(trainee => trainee.Id == traineeId);
            var courseSessionExists = await _dbContext.CourseSessions.AnyAsync(courseSession => courseSession.Id == courseSessionId);

            return traineeExists && courseSessionExists;
        }

        private async Task<Trainee?> GetCurrentTraineeAsync()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return null;
            }

            return await _dbContext.Trainees.FirstOrDefaultAsync(t => t.Email == userEmail);
        }

        private async Task<Instructor?> GetCurrentInstructorAsync()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return null;
            }

            return await _dbContext.Instructors.FirstOrDefaultAsync(i => i.Email == userEmail);
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

        private async Task ValidateEnrollmentBusinessRulesAsync(Enrollment enrollment)
        {
            if (await _dbContext.Enrollments.AnyAsync(e =>
                e.TraineeId == enrollment.TraineeId &&
                e.CourseSessionId == enrollment.CourseSessionId &&
                e.Id != enrollment.Id))
            {
                ModelState.AddModelError("", "This trainee is already enrolled in this session.");
            }

            var session = await _dbContext.CourseSessions
                .Include(cs => cs.Enrollments)
                .Include(cs => cs.Course)
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

            if (session.Course?.PrerequisiteCourseId != null)
            {
                var prerequisiteCompleted = await _dbContext.Enrollments
                    .Include(e => e.CourseSession)
                    .AnyAsync(e =>
                        e.TraineeId == enrollment.TraineeId &&
                        e.CourseSession != null &&
                        e.CourseSession.CourseId == session.Course.PrerequisiteCourseId &&
                        e.Status == "Completed" &&
                        e.ResultStatus == "Pass");

                if (!prerequisiteCompleted)
                {
                    ModelState.AddModelError("", "Trainee has not completed the prerequisite course.");
                }
            }
        }

        private static object BuildEnrollmentResponse(Enrollment enrollment)
        {
            return new
            {
                enrollment.Id,
                enrollment.TraineeId,
                enrollment.CourseSessionId,
                enrollment.Status,
                enrollment.EnrolledAt,
                enrollment.AttendanceStatus,
                enrollment.ResultStatus,
                enrollment.ResultRecordedAt,
                Trainee = enrollment.Trainee == null ? null : new { enrollment.Trainee.Id, enrollment.Trainee.FullName },
                CourseSession = enrollment.CourseSession == null
                    ? null
                    : new
                    {
                        enrollment.CourseSession.Id,
                        enrollment.CourseSession.CourseId,
                        CourseTitle = enrollment.CourseSession.Course?.Title ?? string.Empty,
                        enrollment.CourseSession.StartDate,
                        enrollment.CourseSession.EndDate,
                        enrollment.CourseSession.Status
                    }
            };
        }

    }
}
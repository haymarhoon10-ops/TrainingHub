using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Data;
using TrainingHub.Models;

namespace TrainingHub.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
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

            // If the current user is a Trainee, allow access only to their own enrollments
            if (User.IsInRole("Trainee"))
            {
                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (!string.Equals(userEmail, enrollment.Trainee?.Email, StringComparison.OrdinalIgnoreCase))
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
            var enrollment = await _dbContext.Enrollments.FirstOrDefaultAsync(entity => entity.Id == id);

            if (enrollment == null)
            {
                return NotFound();
            }

            if (!await ReferencesExistAsync(request.TraineeId, request.CourseSessionId))
            {
                return BadRequest(new { message = "The specified trainee or course session does not exist." });
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
        [Authorize(Roles = "TrainingCoordinator,Instructor")]
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
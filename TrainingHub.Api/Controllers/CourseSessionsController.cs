using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Data;
using TrainingHub.Models;

namespace TrainingHub.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/course-sessions")]
    public class CourseSessionsController : ControllerBase
    {
        private readonly TrainingHubDbContext _dbContext;

        public CourseSessionsController(TrainingHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courseSessions = await GetCourseSessionQuery()
                .OrderByDescending(courseSession => courseSession.StartDate)
                .ToListAsync();

            return Ok(courseSessions.Select(BuildCourseSessionResponse));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var courseSession = await GetCourseSessionQuery()
                .FirstOrDefaultAsync(entity => entity.Id == id);

            if (courseSession == null)
            {
                return NotFound();
            }

            return Ok(BuildCourseSessionResponse(courseSession));
        }

        [HttpPost]
        [Authorize(Roles = "TrainingCoordinator")]
        public async Task<IActionResult> Create([FromBody] CourseSession request)
        {
            if (!await ReferencesExistAsync(request.CourseId, request.InstructorId, request.ClassroomId))
            {
                return BadRequest(new { message = "One or more related records do not exist." });
            }

            await ValidateCourseSessionRules(request);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var courseSession = new CourseSession
            {
                CourseId = request.CourseId,
                InstructorId = request.InstructorId,
                ClassroomId = request.ClassroomId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Capacity = request.Capacity,
                Status = request.Status
            };

            _dbContext.CourseSessions.Add(courseSession);
            await _dbContext.SaveChangesAsync();

            var createdCourseSession = await GetCourseSessionQuery()
                .FirstAsync(entity => entity.Id == courseSession.Id);

            return CreatedAtAction(nameof(GetById), new { id = createdCourseSession.Id }, BuildCourseSessionResponse(createdCourseSession));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "TrainingCoordinator")]
        public async Task<IActionResult> Update(int id, [FromBody] CourseSession request)
        {
            var courseSession = await _dbContext.CourseSessions.FirstOrDefaultAsync(entity => entity.Id == id);

            if (courseSession == null)
            {
                return NotFound();
            }

            if (!await ReferencesExistAsync(request.CourseId, request.InstructorId, request.ClassroomId))
            {
                return BadRequest(new { message = "One or more related records do not exist." });
            }

            request.Id = id;
            await ValidateCourseSessionRules(request);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            courseSession.CourseId = request.CourseId;
            courseSession.InstructorId = request.InstructorId;
            courseSession.ClassroomId = request.ClassroomId;
            courseSession.StartDate = request.StartDate;
            courseSession.EndDate = request.EndDate;
            courseSession.Capacity = request.Capacity;
            courseSession.Status = request.Status;

            await _dbContext.SaveChangesAsync();

            var updatedCourseSession = await GetCourseSessionQuery()
                .FirstAsync(entity => entity.Id == id);

            return Ok(BuildCourseSessionResponse(updatedCourseSession));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "TrainingCoordinator")]
        public async Task<IActionResult> Delete(int id)
        {
            var courseSession = await _dbContext.CourseSessions.FirstOrDefaultAsync(entity => entity.Id == id);

            if (courseSession == null)
            {
                return NotFound();
            }

            _dbContext.CourseSessions.Remove(courseSession);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private IQueryable<CourseSession> GetCourseSessionQuery()
        {
            return _dbContext.CourseSessions
                .AsNoTracking()
                .Include(courseSession => courseSession.Course)
                .Include(courseSession => courseSession.Instructor)
                .Include(courseSession => courseSession.Classroom);
        }

        private async Task<bool> ReferencesExistAsync(int courseId, int instructorId, int classroomId)
        {
            var courseExists = await _dbContext.Courses.AnyAsync(course => course.Id == courseId);
            var instructorExists = await _dbContext.Instructors.AnyAsync(instructor => instructor.Id == instructorId);
            var classroomExists = await _dbContext.Classrooms.AnyAsync(classroom => classroom.Id == classroomId);

            return courseExists && instructorExists && classroomExists;
        }

        private async Task ValidateCourseSessionRules(CourseSession courseSession)
        {
            if (courseSession.EndDate <= courseSession.StartDate)
            {
                ModelState.AddModelError("EndDate", "EndDate must be greater than StartDate.");
            }

            var classroom = await _dbContext.Classrooms.FirstOrDefaultAsync(c => c.Id == courseSession.ClassroomId);
            if (classroom != null && courseSession.Capacity > classroom.Capacity)
            {
                ModelState.AddModelError("Capacity", $"Session capacity cannot exceed classroom capacity ({classroom.Capacity}).");
            }

            var instructorConflict = await _dbContext.CourseSessions.AnyAsync(cs =>
                cs.Id != courseSession.Id &&
                cs.InstructorId == courseSession.InstructorId &&
                cs.StartDate < courseSession.EndDate &&
                courseSession.StartDate < cs.EndDate);

            if (instructorConflict)
            {
                ModelState.AddModelError("InstructorId", "This instructor already has another session during this time.");
            }

            var classroomConflict = await _dbContext.CourseSessions.AnyAsync(cs =>
                cs.Id != courseSession.Id &&
                cs.ClassroomId == courseSession.ClassroomId &&
                cs.StartDate < courseSession.EndDate &&
                courseSession.StartDate < cs.EndDate);

            if (classroomConflict)
            {
                ModelState.AddModelError("ClassroomId", "This classroom is already booked during this time.");
            }
        }

        private static object BuildCourseSessionResponse(CourseSession courseSession)
        {
            return new
            {
                courseSession.Id,
                courseSession.CourseId,
                courseSession.InstructorId,
                courseSession.ClassroomId,
                courseSession.StartDate,
                courseSession.EndDate,
                courseSession.Capacity,
                courseSession.Status,
                courseSession.CreatedAt,
                Course = courseSession.Course == null ? null : new { courseSession.Course.Id, courseSession.Course.Title },
                Instructor = courseSession.Instructor == null ? null : new { courseSession.Instructor.Id, courseSession.Instructor.Name },
                Classroom = courseSession.Classroom == null ? null : new { courseSession.Classroom.Id, courseSession.Classroom.RoomCode, courseSession.Classroom.Location }
            };
        }

    }
}
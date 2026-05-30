using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Api.Models;
using TrainingHub.Data;

namespace TrainingHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "TrainingCoordinator")]
    public class ReportsController : ControllerBase
    {
        private readonly TrainingHubDbContext _dbContext;

        public ReportsController(TrainingHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("instructors")]
        public async Task<ActionResult<IEnumerable<InstructorWorkloadResponse>>> GetInstructorWorkload()
        {
            /* * Execute a projection query against the database to calculate instructor metrics.
             * This minimizes memory usage by only returning the specific fields required by the client.
             */
            var workloadData = await _dbContext.Instructors
                .Include(i => i.CourseSessions)
                    .ThenInclude(cs => cs.Enrollments)
                .Select(instructor => new InstructorWorkloadResponse(
                    instructor.Name,

                    string.IsNullOrEmpty(instructor.ExpertiseArea) ? "General" : instructor.ExpertiseArea,

                    // (Note: If your CourseSession model uses a boolean like 'IsActive' instead of a string 'Status', update this line accordingly)
                    instructor.CourseSessions.Count(cs => cs.Status == "1"),

                    instructor.CourseSessions.SelectMany(cs => cs.Enrollments).Count()
                ))
                .ToListAsync();
            return Ok(workloadData);
        }

        [HttpGet("enrollments")]
        public async Task<ActionResult<IEnumerable<EnrollmentStatResponse>>> GetEnrollments()
        {
            // Queries the database to aggregate capacity and enrollment totals per course
            var stats = await _dbContext.Courses
                .Include(c => c.Category)
                .Include(c => c.CourseSessions)
                    .ThenInclude(cs => cs.Enrollments)
                .Select(course => new EnrollmentStatResponse(
                    course.Title,
                    course.Category != null ? course.Category.Name : "Uncategorized",
                    course.CourseSessions.Sum(cs => cs.Capacity),
                    course.CourseSessions.SelectMany(cs => cs.Enrollments).Count()
                ))
                .ToListAsync();

            return Ok(stats);
        }
    }
}
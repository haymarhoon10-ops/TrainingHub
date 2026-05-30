using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Api.DTOs;
using TrainingHub.Data;
using TrainingHub.Models;

namespace TrainingHub.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/courses")]
    [Produces("application/json")]
    public class CoursesController : ControllerBase
    {
        private readonly TrainingHubDbContext _dbContext;

        public CoursesController(TrainingHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courses = await _dbContext.Courses
                .AsNoTracking()
                .Include(course => course.Category)
                .Include(course => course.PrerequisiteCourse)
                .OrderBy(course => course.Title)
                .ToListAsync();

            return Ok(courses.Select(BuildCourseResponse).ToList());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await GetCourseQuery()
                .FirstOrDefaultAsync(course => course.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return Ok(BuildCourseResponse(course));
        }

        [HttpPost]
        [Authorize(Roles = "TrainingCoordinator")]
        public async Task<IActionResult> Create([FromBody] Course request)
        {
            if (!await CategoryExistsAsync(request.CategoryId))
            {
                return BadRequest(new ErrorResponse { Message = "The specified category does not exist." });
            }

            if (request.PrerequisiteCourseId.HasValue)
            {
                if (request.PrerequisiteCourseId.Value == request.Id)
                {
                    return BadRequest(new ErrorResponse { Message = "A course cannot be its own prerequisite." });
                }

                if (!await CourseExistsAsync(request.PrerequisiteCourseId.Value))
                {
                    return BadRequest(new { message = "The specified prerequisite course does not exist." });
                }
            }

            var course = new Course
            {
                Title = request.Title,
                Description = request.Description,
                DurationHours = request.DurationHours,
                DefaultCapacity = request.DefaultCapacity,
                Fee = request.Fee,
                IsActive = request.IsActive,
                CategoryId = request.CategoryId,
                PrerequisiteCourseId = request.PrerequisiteCourseId
            };

            _dbContext.Courses.Add(course);
            await _dbContext.SaveChangesAsync();

            var createdCourse = await GetCourseQuery()
                .FirstAsync(entity => entity.Id == course.Id);

            return CreatedAtAction(nameof(GetById), new { id = createdCourse.Id }, BuildCourseResponse(createdCourse));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "TrainingCoordinator")]
        public async Task<IActionResult> Update(int id, [FromBody] Course request)
        {
            var course = await _dbContext.Courses.FirstOrDefaultAsync(entity => entity.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            if (!await CategoryExistsAsync(request.CategoryId))
            {
                return BadRequest(new ErrorResponse { Message = "The specified category does not exist." });
            }

            if (request.PrerequisiteCourseId.HasValue)
            {
                if (request.PrerequisiteCourseId.Value == id)
                {
                    return BadRequest(new ErrorResponse { Message = "A course cannot be its own prerequisite." });
                }

                if (!await CourseExistsAsync(request.PrerequisiteCourseId.Value))
                {
                    return BadRequest(new ErrorResponse { Message = "The specified prerequisite course does not exist." });
                }
            }

            course.Title = request.Title;
            course.Description = request.Description;
            course.DurationHours = request.DurationHours;
            course.DefaultCapacity = request.DefaultCapacity;
            course.Fee = request.Fee;
            course.IsActive = request.IsActive;
            course.CategoryId = request.CategoryId;
            course.PrerequisiteCourseId = request.PrerequisiteCourseId;

            await _dbContext.SaveChangesAsync();

            var updatedCourse = await GetCourseQuery()
                .FirstAsync(entity => entity.Id == id);

            return Ok(BuildCourseResponse(updatedCourse));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "TrainingCoordinator")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _dbContext.Courses.FirstOrDefaultAsync(entity => entity.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            _dbContext.Courses.Remove(course);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private IQueryable<Course> GetCourseQuery()
        {
            return _dbContext.Courses
                .AsNoTracking()
                .Include(course => course.Category)
                .Include(course => course.PrerequisiteCourse);
        }

        private Task<bool> CategoryExistsAsync(int categoryId)
        {
            return _dbContext.Categories.AnyAsync(category => category.Id == categoryId);
        }

        private Task<bool> CourseExistsAsync(int courseId)
        {
            return _dbContext.Courses.AnyAsync(course => course.Id == courseId);
        }

        private static CourseResponse BuildCourseResponse(Course course)
        {
            return new CourseResponse
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                DurationHours = course.DurationHours,
                DefaultCapacity = course.DefaultCapacity,
                Fee = course.Fee,
                IsActive = course.IsActive,
                CategoryId = course.CategoryId,
                CategoryName = course.Category?.Name ?? string.Empty,
                PrerequisiteCourseId = course.PrerequisiteCourseId,
                PrerequisiteCourseName = course.PrerequisiteCourse?.Title
            };
        }

    }
}
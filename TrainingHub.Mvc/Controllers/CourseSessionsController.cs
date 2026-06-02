using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Data;
using TrainingHub.Models;
using TrainingHub.Mvc.Services;
using TrainingHub.Security;

namespace TrainingHub.Mvc.Controllers
{
    [Authorize(Roles = RoleNames.TrainingCoordinator + "," + RoleNames.Instructor + "," + RoleNames.Trainee)]
    public class CourseSessionsController : Controller
    {
        private readonly TrainingHubDbContext _context;
        private readonly IRealtimeNotifier _realtimeNotifier;

        public CourseSessionsController(TrainingHubDbContext context, IRealtimeNotifier realtimeNotifier)
        {
            _context = context;
            _realtimeNotifier = realtimeNotifier;
        }

        public async Task<IActionResult> Index()
        {
            var currentEmail = User.Identity?.Name;
            IQueryable<CourseSession> sessions = _context.CourseSessions
                .Include(c => c.Course)
                .Include(c => c.Instructor)
                .Include(c => c.Classroom);

            if (User.IsInRole(RoleNames.Instructor) && !User.IsInRole(RoleNames.TrainingCoordinator))
            {
                sessions = sessions.Where(session => session.Instructor != null && session.Instructor.Email == currentEmail);
            }

            return View(await sessions.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var courseSession = await _context.CourseSessions
                .Include(c => c.Course)
                .Include(c => c.Instructor)
                .Include(c => c.Classroom)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (courseSession == null) return NotFound();

            if (User.IsInRole(RoleNames.Instructor) && !User.IsInRole(RoleNames.TrainingCoordinator))
            {
                var currentEmail = User.Identity?.Name;
                if (!string.Equals(courseSession.Instructor?.Email, currentEmail, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }
            }

            return View(courseSession);
        }

        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View(new CourseSession());
        }

        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseId,InstructorId,ClassroomId,StartDate,EndDate,Capacity,Status")] CourseSession courseSession)
        {
            courseSession.CreatedAt = DateTime.Now;

            await ValidateCourseSessionRules(courseSession);

            if (ModelState.IsValid)
            {
                _context.Add(courseSession);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(courseSession);
            return View(courseSession);
        }

        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var courseSession = await _context.CourseSessions.FindAsync(id);

            if (courseSession == null) return NotFound();

            PopulateDropdowns(courseSession);
            return View(courseSession);
        }

        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseId,InstructorId,ClassroomId,StartDate,EndDate,Capacity,Status,CreatedAt")] CourseSession courseSession)
        {
            if (id != courseSession.Id) return NotFound();

            await ValidateCourseSessionRules(courseSession);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(courseSession);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseSessionExists(courseSession.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(courseSession);
            return View(courseSession);
        }

        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var courseSession = await _context.CourseSessions
                .Include(c => c.Course)
                .Include(c => c.Instructor)
                .Include(c => c.Classroom)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (courseSession == null) return NotFound();

            return View(courseSession);
        }

        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var courseSession = await _context.CourseSessions
                .Include(cs => cs.Enrollments)
                .FirstOrDefaultAsync(cs => cs.Id == id);

            if (courseSession == null) return NotFound();

            if (courseSession.Enrollments.Any())
            {
                ModelState.AddModelError("", "Cannot delete this session because trainees are enrolled.");
                return View("Delete", courseSession);
            }

            _context.CourseSessions.Remove(courseSession);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateCourseSessionRules(CourseSession courseSession)
        {
            if (courseSession.EndDate <= courseSession.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
            }

            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.Id == courseSession.ClassroomId);

            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseSession.CourseId);

            if (classroom != null && courseSession.Capacity > classroom.Capacity)
            {
                ModelState.AddModelError("Capacity", $"Session capacity cannot exceed classroom capacity ({classroom.Capacity}).");
            }

            if (course != null && classroom != null)
            {
                if (course.RequiresProjector && !classroom.HasProjector)
                {
                    ModelState.AddModelError("ClassroomId", "This course requires a projector, but the selected classroom does not have one.");
                }

                if (course.RequiresLabComputer && !classroom.HasLabComputer)
                {
                    ModelState.AddModelError("ClassroomId", "This course requires lab computers, but the selected classroom does not provide them.");
                }
            }

            var instructorConflict = await _context.CourseSessions.AnyAsync(cs =>
                cs.Id != courseSession.Id &&
                cs.InstructorId == courseSession.InstructorId &&
                cs.StartDate < courseSession.EndDate &&
                courseSession.StartDate < cs.EndDate);

            if (instructorConflict)
            {
                ModelState.AddModelError("InstructorId", "This instructor already has another session during this time.");
            }

            var classroomConflict = await _context.CourseSessions.AnyAsync(cs =>
                cs.Id != courseSession.Id &&
                cs.ClassroomId == courseSession.ClassroomId &&
                cs.StartDate < courseSession.EndDate &&
                courseSession.StartDate < cs.EndDate);

            if (classroomConflict)
            {
                ModelState.AddModelError("ClassroomId", "This classroom is already booked during this time.");
            }
        }

        private void PopulateDropdowns(CourseSession? courseSession = null)
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", courseSession?.CourseId);
            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "Name", courseSession?.InstructorId);
            ViewData["ClassroomId"] = new SelectList(_context.Classrooms, "Id", "Location", courseSession?.ClassroomId);
        }

        private bool CourseSessionExists(int id)
        {
            return _context.CourseSessions.Any(e => e.Id == id);
        }
    }
}
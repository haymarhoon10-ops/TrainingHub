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
using TrainingHub.Security;

namespace TrainingHub.Mvc.Controllers
{
    // All authenticated users can view courses; only TrainingCoordinator can create/edit/delete
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly TrainingHubDbContext _context;

        public CoursesController(TrainingHubDbContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            var trainingHubDbContext = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.PrerequisiteCourse);
            return View(await trainingHubDbContext.ToListAsync());
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.PrerequisiteCourse)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        public IActionResult Create()
        {
            PopulateCourseDropdowns();
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,DurationHours,DefaultCapacity,Fee,IsActive,CategoryId,PrerequisiteCourseId,RequiresProjector,RequiresLabComputer")] Course course)
        {
            ValidateCourseRequirements(course);

            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateCourseDropdowns(course);
            return View(course);
        }

        // GET: Courses/Edit/5
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            PopulateCourseDropdowns(course);
            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,DurationHours,DefaultCapacity,Fee,IsActive,CategoryId,PrerequisiteCourseId,RequiresProjector,RequiresLabComputer")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            ValidateCourseRequirements(course);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateCourseDropdowns(course);
            return View(course);
        }

        // GET: Courses/Delete/5
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.PrerequisiteCourse)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses
                .Include(c => c.CourseSessions)
                .Include(c => c.CertificationTrackCourses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            if (course.CourseSessions.Any() ||
                course.CertificationTrackCourses.Any())
            {
                ModelState.AddModelError("", "Cannot delete this course because it is connected to sessions or certification tracks.");
                return View("Delete", course);
            }

            try
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete this course because it is connected to other records.");
                return View("Delete", course);
            }
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }

        private void PopulateCourseDropdowns(Course? course = null)
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description", course?.CategoryId);
            ViewData["PrerequisiteCourseId"] = new SelectList(
                _context.Courses
                    .Where(existingCourse => course == null || existingCourse.Id != course.Id)
                    .OrderBy(existingCourse => existingCourse.Title),
                "Id",
                "Title",
                course?.PrerequisiteCourseId);
        }

        private void ValidateCourseRequirements(Course course)
        {
            if (course.PrerequisiteCourseId == course.Id && course.PrerequisiteCourseId.HasValue)
            {
                ModelState.AddModelError(nameof(Course.PrerequisiteCourseId), "A course cannot be its own prerequisite.");
            }
        }
    }
}

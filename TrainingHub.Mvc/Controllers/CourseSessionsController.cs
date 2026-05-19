using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Data;
using TrainingHub.Models;

namespace TrainingHub.Mvc.Controllers
{
    public class CourseSessionsController : Controller
    {
        private readonly TrainingHubDbContext _context;

        public CourseSessionsController(TrainingHubDbContext context)
        {
            _context = context;
        }

        // GET: CourseSessions
        public async Task<IActionResult> Index()
        {
            var trainingHubDbContext = _context.CourseSessions
                .Include(c => c.Classroom)
                .Include(c => c.Course)
                .Include(c => c.Instructor);

            return View(await trainingHubDbContext.ToListAsync());
        }

        // GET: CourseSessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var courseSession = await _context.CourseSessions
                .Include(c => c.Classroom)
                .Include(c => c.Course)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (courseSession == null)
                return NotFound();

            return View(courseSession);
        }

        // GET: CourseSessions/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: CourseSessions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CourseId,InstructorId,ClassroomId,StartDate,EndDate,Capacity,Status,CreatedAt")] CourseSession courseSession)
        {
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

        // GET: CourseSessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var courseSession = await _context.CourseSessions.FindAsync(id);

            if (courseSession == null)
                return NotFound();

            PopulateDropdowns(courseSession);
            return View(courseSession);
        }

        // POST: CourseSessions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseId,InstructorId,ClassroomId,StartDate,EndDate,Capacity,Status,CreatedAt")] CourseSession courseSession)
        {
            if (id != courseSession.Id)
                return NotFound();

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
                    if (!CourseSessionExists(courseSession.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(courseSession);
            return View(courseSession);
        }

        // GET: CourseSessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var courseSession = await _context.CourseSessions
                .Include(c => c.Classroom)
                .Include(c => c.Course)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (courseSession == null)
                return NotFound();

            return View(courseSession);
        }

        // POST: CourseSessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var courseSession = await _context.CourseSessions
                .Include(cs => cs.Enrollments)
                .FirstOrDefaultAsync(cs => cs.Id == id);

            if (courseSession == null)
                return NotFound();

            if (courseSession.Enrollments.Any())
            {
                ModelState.AddModelError("", "Cannot delete this session because students are enrolled.");
                return View("Delete", courseSession);
            }

            try
            {
                _context.CourseSessions.Remove(courseSession);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete this session because related records exist.");
                return View("Delete", courseSession);
            }
        }

        private async Task ValidateCourseSessionRules(CourseSession courseSession)
        {
            if (courseSession.EndDate <= courseSession.StartDate)
            {
                ModelState.AddModelError("", "End date must be after start date.");
            }

            var instructorBooked = await _context.CourseSessions.AnyAsync(cs =>
                cs.InstructorId == courseSession.InstructorId &&
                cs.Id != courseSession.Id &&
                cs.StartDate < courseSession.EndDate &&
                courseSession.StartDate < cs.EndDate);

            if (instructorBooked)
            {
                ModelState.AddModelError("", "This instructor is already booked during this time.");
            }

            var classroomBooked = await _context.CourseSessions.AnyAsync(cs =>
                cs.ClassroomId == courseSession.ClassroomId &&
                cs.Id != courseSession.Id &&
                cs.StartDate < courseSession.EndDate &&
                courseSession.StartDate < cs.EndDate);

            if (classroomBooked)
            {
                ModelState.AddModelError("", "This classroom is already booked during this time.");
            }

            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.Id == courseSession.ClassroomId);

            if (classroom != null && courseSession.Capacity > classroom.Capacity)
            {
                ModelState.AddModelError("", "Session capacity cannot exceed classroom capacity.");
            }
        }

        private void PopulateDropdowns(CourseSession? courseSession = null)
        {
            ViewData["ClassroomId"] = new SelectList(_context.Classrooms, "Id", "Location", courseSession?.ClassroomId);
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Description", courseSession?.CourseId);
            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "Bio", courseSession?.InstructorId);
        }

        private bool CourseSessionExists(int id)
        {
            return _context.CourseSessions.Any(e => e.Id == id);
        }
    }
}
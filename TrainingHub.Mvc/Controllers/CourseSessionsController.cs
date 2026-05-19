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
            var trainingHubDbContext = _context.CourseSessions.Include(c => c.Classroom).Include(c => c.Course).Include(c => c.Instructor);
            return View(await trainingHubDbContext.ToListAsync());
        }

        // GET: CourseSessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseSession = await _context.CourseSessions
                .Include(c => c.Classroom)
                .Include(c => c.Course)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courseSession == null)
            {
                return NotFound();
            }

            return View(courseSession);
        }

        // GET: CourseSessions/Create
        public IActionResult Create()
        {
            ViewData["ClassroomId"] = new SelectList(_context.Classrooms, "Id", "Location");
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Description");
            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "Bio");
            return View();
        }

        // POST: CourseSessions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CourseId,InstructorId,ClassroomId,StartDate,EndDate,Capacity,Status,CreatedAt")] CourseSession courseSession)
        {
            if (ModelState.IsValid)
            {
                _context.Add(courseSession);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClassroomId"] = new SelectList(_context.Classrooms, "Id", "Location", courseSession.ClassroomId);
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Description", courseSession.CourseId);
            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "Bio", courseSession.InstructorId);
            return View(courseSession);
        }

        // GET: CourseSessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseSession = await _context.CourseSessions.FindAsync(id);
            if (courseSession == null)
            {
                return NotFound();
            }
            ViewData["ClassroomId"] = new SelectList(_context.Classrooms, "Id", "Location", courseSession.ClassroomId);
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Description", courseSession.CourseId);
            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "Bio", courseSession.InstructorId);
            return View(courseSession);
        }

        // POST: CourseSessions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseId,InstructorId,ClassroomId,StartDate,EndDate,Capacity,Status,CreatedAt")] CourseSession courseSession)
        {
            if (id != courseSession.Id)
            {
                return NotFound();
            }

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
            ViewData["ClassroomId"] = new SelectList(_context.Classrooms, "Id", "Location", courseSession.ClassroomId);
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Description", courseSession.CourseId);
            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "Bio", courseSession.InstructorId);
            return View(courseSession);
        }

        // GET: CourseSessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseSession = await _context.CourseSessions
                .Include(c => c.Classroom)
                .Include(c => c.Course)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courseSession == null)
            {
                return NotFound();
            }

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

        private bool CourseSessionExists(int id)
        {
            return _context.CourseSessions.Any(e => e.Id == id);
        }
    }
}

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
    public class CertificationTrackCoursesController : Controller
    {
        private readonly TrainingHubDbContext _context;

        public CertificationTrackCoursesController(TrainingHubDbContext context)
        {
            _context = context;
        }

        // GET: CertificationTrackCourses
        public async Task<IActionResult> Index()
        {
            var trainingHubDbContext = _context.CertificationTrackCourses.Include(c => c.CertificationTrack).Include(c => c.Course);
            return View(await trainingHubDbContext.ToListAsync());
        }

        // GET: CertificationTrackCourses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificationTrackCourse = await _context.CertificationTrackCourses
                .Include(c => c.CertificationTrack)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificationTrackCourse == null)
            {
                return NotFound();
            }

            return View(certificationTrackCourse);
        }

        // GET: CertificationTrackCourses/Create
        public IActionResult Create()
        {
            ViewData["CertificationTrackId"] = new SelectList(_context.CertificationTracks, "Id", "Description");
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Description");
            return View();
        }

        // POST: CertificationTrackCourses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CertificationTrackId,CourseId")] CertificationTrackCourse certificationTrackCourse)
        {
            if (ModelState.IsValid)
            {
                _context.Add(certificationTrackCourse);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CertificationTrackId"] = new SelectList(_context.CertificationTracks, "Id", "Description", certificationTrackCourse.CertificationTrackId);
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Description", certificationTrackCourse.CourseId);
            return View(certificationTrackCourse);
        }

        // GET: CertificationTrackCourses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificationTrackCourse = await _context.CertificationTrackCourses.FindAsync(id);
            if (certificationTrackCourse == null)
            {
                return NotFound();
            }
            ViewData["CertificationTrackId"] = new SelectList(_context.CertificationTracks, "Id", "Description", certificationTrackCourse.CertificationTrackId);
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Description", certificationTrackCourse.CourseId);
            return View(certificationTrackCourse);
        }

        // POST: CertificationTrackCourses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CertificationTrackId,CourseId")] CertificationTrackCourse certificationTrackCourse)
        {
            if (id != certificationTrackCourse.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(certificationTrackCourse);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CertificationTrackCourseExists(certificationTrackCourse.Id))
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
            ViewData["CertificationTrackId"] = new SelectList(_context.CertificationTracks, "Id", "Description", certificationTrackCourse.CertificationTrackId);
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Description", certificationTrackCourse.CourseId);
            return View(certificationTrackCourse);
        }

        // GET: CertificationTrackCourses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificationTrackCourse = await _context.CertificationTrackCourses
                .Include(c => c.CertificationTrack)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificationTrackCourse == null)
            {
                return NotFound();
            }

            return View(certificationTrackCourse);
        }

        // POST: CertificationTrackCourses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var certificationTrackCourse = await _context.CertificationTrackCourses.FindAsync(id);

            if (certificationTrackCourse == null)
                return NotFound();

            try
            {
                _context.CertificationTrackCourses.Remove(certificationTrackCourse);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete record because related data exists.");
                return View("Delete", certificationTrackCourse);
            }
        }

        private bool CertificationTrackCourseExists(int id)
        {
            return _context.CertificationTrackCourses.Any(e => e.Id == id);
        }
    }
}

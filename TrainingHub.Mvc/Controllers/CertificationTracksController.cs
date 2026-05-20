using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Data;
using TrainingHub.Models;
using TrainingHub.Mvc.Models;

namespace TrainingHub.Mvc.Controllers
{
    public class CertificationTracksController : Controller
    {
        private readonly TrainingHubDbContext _context;

        public CertificationTracksController(TrainingHubDbContext context)
        {
            _context = context;
        }

        // GET: CertificationTracks
        public async Task<IActionResult> Index()
        {
            return View(await _context.CertificationTracks.ToListAsync());
        }

        // GET: CertificationTracks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificationTrack = await _context.CertificationTracks
    .Include(ct => ct.CertificationTrackCourses)
        .ThenInclude(ctc => ctc.Course)
    .FirstOrDefaultAsync(m => m.Id == id);
            if (certificationTrack == null)
            {
                return NotFound();
            }

            return View(certificationTrack);
        }

        // GET: CertificationTracks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CertificationTracks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,IsActive")] CertificationTrack certificationTrack)
        {
            if (ModelState.IsValid)
            {
                _context.Add(certificationTrack);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(certificationTrack);
        }

        // GET: CertificationTracks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificationTrack = await _context.CertificationTracks.FindAsync(id);
            if (certificationTrack == null)
            {
                return NotFound();
            }
            return View(certificationTrack);
        }

        // POST: CertificationTracks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsActive")] CertificationTrack certificationTrack)
        {
            if (id != certificationTrack.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(certificationTrack);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CertificationTrackExists(certificationTrack.Id))
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
            return View(certificationTrack);
        }

        // GET: CertificationTracks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificationTrack = await _context.CertificationTracks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificationTrack == null)
            {
                return NotFound();
            }

            return View(certificationTrack);
        }

        // POST: CertificationTracks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var certificationTrack = await _context.CertificationTracks
                .Include(ct => ct.CertificationTrackCourses)
                .Include(ct => ct.Certificates)
                .FirstOrDefaultAsync(ct => ct.Id == id);

            if (certificationTrack == null)
                return NotFound();

            if (certificationTrack.CertificationTrackCourses.Any() ||
                certificationTrack.Certificates.Any())
            {
                ModelState.AddModelError("", "Cannot delete this certification track because it is connected to courses or certificates.");
                return View("Delete", certificationTrack);
            }

            try
            {
                _context.CertificationTracks.Remove(certificationTrack);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete this certification track because related records exist.");
                return View("Delete", certificationTrack);
            }
        }

        // GET: CertificationTracks/Progress?traineeId=1&trackId=1
        public async Task<IActionResult> Progress(int? traineeId, int? trackId)
        {
            if (traineeId == null || trackId == null)
            {
                return BadRequest("Trainee ID and Track ID are required.");
            }

            var trainee = await _context.Trainees.FindAsync(traineeId);
            if (trainee == null)
            {
                return NotFound("Trainee not found.");
            }

            var certificationTrack = await _context.CertificationTracks
                .Include(ct => ct.CertificationTrackCourses)
                    .ThenInclude(ctc => ctc.Course)
                .FirstOrDefaultAsync(ct => ct.Id == trackId);

            if (certificationTrack == null)
            {
                return NotFound("Certification Track not found.");
            }

            // Get total required courses in the track
            int totalCourses = certificationTrack.CertificationTrackCourses.Count;

            // Get completed courses for the trainee
            var completedCourses = await _context.Enrollments
                .Where(e =>
                    e.TraineeId == traineeId &&
                    e.Status == "Completed" &&
                    e.ResultStatus == "Pass" &&
                    certificationTrack.CertificationTrackCourses
                        .Select(ctc => ctc.CourseId)
                        .Contains(e.CourseSession!.CourseId))
                .Select(e => e.CourseSession!.CourseId)
                .Distinct()
                .CountAsync();

            // Calculate progress percentage
            decimal progressPercentage = totalCourses > 0
                ? Math.Round((decimal)completedCourses / totalCourses * 100, 2)
                : 0;

            var viewModel = new CertificationProgressViewModel
            {
                TraineeName = trainee.FullName,
                CertificationTrackName = certificationTrack.Name,
                CompletedCourses = completedCourses,
                TotalRequiredCourses = totalCourses,
                ProgressPercentage = progressPercentage
            };

            return View(viewModel);
        }

        private bool CertificationTrackExists(int id)
        {
            return _context.CertificationTracks.Any(e => e.Id == id);
        }
    }
}

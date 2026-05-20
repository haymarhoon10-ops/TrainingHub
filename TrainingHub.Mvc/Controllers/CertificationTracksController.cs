using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Data;
using TrainingHub.Models;

namespace TrainingHub.Mvc.Controllers
{
    public class CertificationTracksController : Controller
    {
        private readonly TrainingHubDbContext _context;

        public CertificationTracksController(TrainingHubDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.CertificationTracks.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var certificationTrack = await _context.CertificationTracks
                .Include(ct => ct.CertificationTrackCourses)
                    .ThenInclude(ctc => ctc.Course)
                .FirstOrDefaultAsync(ct => ct.Id == id);

            if (certificationTrack == null)
                return NotFound();

            return View(certificationTrack);
        }

        public IActionResult Create()
        {
            return View();
        }

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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var certificationTrack = await _context.CertificationTracks.FindAsync(id);

            if (certificationTrack == null)
                return NotFound();

            return View(certificationTrack);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsActive")] CertificationTrack certificationTrack)
        {
            if (id != certificationTrack.Id)
                return NotFound();

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
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(certificationTrack);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var certificationTrack = await _context.CertificationTracks
                .FirstOrDefaultAsync(ct => ct.Id == id);

            if (certificationTrack == null)
                return NotFound();

            return View(certificationTrack);
        }

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

            if (certificationTrack.CertificationTrackCourses.Any() || certificationTrack.Certificates.Any())
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

        public async Task<IActionResult> Progress(int? traineeId, int? trackId)
        {
            if (traineeId == null || trackId == null)
            {
                ViewBag.ErrorMessage = "Trainee ID and Track ID are required.";
                return View();
            }

            var trainee = await _context.Trainees
                .FirstOrDefaultAsync(t => t.Id == traineeId);

            if (trainee == null)
            {
                ViewBag.ErrorMessage = "Trainee not found.";
                return View();
            }

            var track = await _context.CertificationTracks
                .Include(ct => ct.CertificationTrackCourses)
                    .ThenInclude(ctc => ctc.Course)
                .FirstOrDefaultAsync(ct => ct.Id == trackId);

            if (track == null)
            {
                ViewBag.ErrorMessage = "Certification Track not found.";
                return View();
            }

            var requiredCourseIds = track.CertificationTrackCourses
                .Select(ctc => ctc.CourseId)
                .ToList();

            var completedCourseIds = await _context.Enrollments
                .Include(e => e.CourseSession)
                .Where(e =>
                    e.TraineeId == traineeId &&
                    e.Status == "Completed" &&
                    e.ResultStatus == "Pass")
                .Select(e => e.CourseSession!.CourseId)
                .Distinct()
                .ToListAsync();

            int totalRequired = requiredCourseIds.Count;
            int completedCount = requiredCourseIds.Count(id => completedCourseIds.Contains(id));

            double progressPercentage = totalRequired == 0
                ? 0
                : (double)completedCount / totalRequired * 100;

            ViewBag.TraineeName = trainee.FullName;
            ViewBag.CompletedCount = completedCount;
            ViewBag.TotalRequired = totalRequired;
            ViewBag.ProgressPercentage = Math.Round(progressPercentage, 0);

            return View(track);
        }

        private bool CertificationTrackExists(int id)
        {
            return _context.CertificationTracks.Any(e => e.Id == id);
        }
    }
}
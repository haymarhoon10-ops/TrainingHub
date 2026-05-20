using System;
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
    public class CertificatesController : Controller
    {
        private readonly TrainingHubDbContext _context;

        public CertificatesController(TrainingHubDbContext context)
        {
            _context = context;
        }

        // GET: Certificates
        public async Task<IActionResult> Index()
        {
            var certificates = await _context.Certificates
                .Include(c => c.CertificationTrack)
                .Include(c => c.Trainee)
                .ToListAsync();

            var passCounts = new Dictionary<int, int>();
            var totalRequired = new Dictionary<int, int>();
            var remaining = new Dictionary<int, int>();
            var isEligible = new Dictionary<int, bool>();

            foreach (var cert in certificates)
            {
                var requiredCourseIds = _context.CertificationTrackCourses
                    .Where(ctc => ctc.CertificationTrackId == cert.CertificationTrackId)
                    .Select(ctc => ctc.CourseId)
                    .ToList();

                var total = requiredCourseIds.Count;

                var passed = _context.Enrollments
                    .Include(e => e.CourseSession)
                    .Where(e => e.TraineeId == cert.TraineeId
                                && e.ResultStatus == "Pass"
                                && e.CourseSession != null
                                && requiredCourseIds.Contains(e.CourseSession.CourseId))
                    .Select(e => e.CourseSession!.CourseId)
                    .Distinct()
                    .Count();

                var rem = Math.Max(0, total - passed);
                var eligible = passed >= total && total > 0;

                passCounts[cert.Id] = passed;
                totalRequired[cert.Id] = total;
                remaining[cert.Id] = rem;
                isEligible[cert.Id] = eligible;
            }

            ViewBag.PassCounts = passCounts;
            ViewBag.TotalRequired = totalRequired;
            ViewBag.Remaining = remaining;
            ViewBag.IsEligible = isEligible;

            return View(certificates);
        }

        // GET: Certificates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificates
                .Include(c => c.CertificationTrack)
                .Include(c => c.Trainee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificate == null)
            {
                return NotFound();
            }

            // compute progress for this certificate
            var requiredCourseIds = _context.CertificationTrackCourses
                .Where(ctc => ctc.CertificationTrackId == certificate.CertificationTrackId)
                .Select(ctc => ctc.CourseId)
                .ToList();

            var total = requiredCourseIds.Count;

            var passed = _context.Enrollments
                .Include(e => e.CourseSession)
                .Where(e => e.TraineeId == certificate.TraineeId
                            && e.ResultStatus == "Pass"
                            && e.CourseSession != null
                            && requiredCourseIds.Contains(e.CourseSession.CourseId))
                .Select(e => e.CourseSession!.CourseId)
                .Distinct()
                .Count();

            var rem = Math.Max(0, total - passed);
            var eligible = passed >= total && total > 0;

            ViewBag.PassedCourses = passed;
            ViewBag.TotalRequired = total;
            ViewBag.RemainingCourses = rem;
            ViewBag.IsEligible = eligible;

            return View(certificate);
        }

        // GET: Certificates/Create
        public IActionResult Create()
        {
            ViewData["CertificationTrackId"] = new SelectList(_context.CertificationTracks, "Id", "Description");
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email");
            return View();
        }

        // POST: Certificates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TraineeId,CertificationTrackId,CertificateReferenceNumber,IssuedAt,Status")] Certificate certificate)
        {
            // basic validation
            if (certificate.TraineeId == 0 || certificate.CertificationTrackId == 0)
            {
                ModelState.AddModelError("", "Please select trainee and certification track.");
            }

            // compute certification progress
            var requiredCourseIds = _context.CertificationTrackCourses
                .Where(ctc => ctc.CertificationTrackId == certificate.CertificationTrackId)
                .Select(ctc => ctc.CourseId)
                .ToList();

            var total = requiredCourseIds.Count;

            var passed = _context.Enrollments
                .Include(e => e.CourseSession)
                .Where(e => e.TraineeId == certificate.TraineeId
                            && e.ResultStatus == "Pass"
                            && e.CourseSession != null
                            && requiredCourseIds.Contains(e.CourseSession.CourseId))
                .Select(e => e.CourseSession!.CourseId)
                .Distinct()
                .Count();

            var rem = Math.Max(0, total - passed);
            var eligible = passed >= total && total > 0;

            // prevent duplicate certificate for same trainee and track
            var exists = _context.Certificates.Any(c => c.TraineeId == certificate.TraineeId && c.CertificationTrackId == certificate.CertificationTrackId);
            if (exists)
            {
                ModelState.AddModelError("", "This trainee already has a certificate for the selected certification track.");
            }

            if (!eligible)
            {
                ModelState.AddModelError("", "Trainee is not eligible for this certificate. They must pass all required courses.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["CertificationTrackId"] = new SelectList(_context.CertificationTracks, "Id", "Description", certificate.CertificationTrackId);
                ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email", certificate.TraineeId);

                // provide progress info back to view
                ViewBag.PassedCourses = passed;
                ViewBag.TotalRequired = total;
                ViewBag.RemainingCourses = rem;
                ViewBag.IsEligible = eligible;

                return View(certificate);
            }

            // create certificate
            certificate.CertificateReferenceNumber = GenerateCertificateReference();
            certificate.Status = "Issued";
            certificate.IssuedAt = DateTime.Now;

            _context.Add(certificate);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Certificates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null)
            {
                return NotFound();
            }
            ViewData["CertificationTrackId"] = new SelectList(_context.CertificationTracks, "Id", "Description", certificate.CertificationTrackId);
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email", certificate.TraineeId);
            return View(certificate);
        }

        // POST: Certificates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TraineeId,CertificationTrackId,CertificateReferenceNumber,IssuedAt,Status")] Certificate certificate)
        {
            if (id != certificate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(certificate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CertificateExists(certificate.Id))
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
            ViewData["CertificationTrackId"] = new SelectList(_context.CertificationTracks, "Id", "Description", certificate.CertificationTrackId);
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email", certificate.TraineeId);
            return View(certificate);
        }

        // GET: Certificates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificates
                .Include(c => c.CertificationTrack)
                .Include(c => c.Trainee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificate == null)
            {
                return NotFound();
            }

            return View(certificate);
        }

        // POST: Certificates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var certificate = await _context.Certificates.FindAsync(id);

            if (certificate == null)
                return NotFound();

            try
            {
                _context.Certificates.Remove(certificate);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete certificate because related records exist.");
                return View("Delete", certificate);
            }
        }

        private bool CertificateExists(int id)
        {
            return _context.Certificates.Any(e => e.Id == id);
        }

        private string GenerateCertificateReference()
        {
            // simple unique reference using timestamp and random
            var ts = DateTime.Now.ToString("yyyyMMddHHmmss");
            var rand = new Random().Next(1000, 9999);
            return $"CERT-{ts}-{rand}";
        }
    }
}

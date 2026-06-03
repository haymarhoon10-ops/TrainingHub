using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Data;
using TrainingHub.Mvc.Models;
using TrainingHub.Models;
using TrainingHub.Security;

namespace TrainingHub.Mvc.Controllers
{
    [Authorize(Roles = RoleNames.TrainingCoordinator + "," + RoleNames.Trainee)]
    public class CertificatesController : Controller
    {
        private readonly TrainingHubDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public CertificatesController(TrainingHubDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // GET: Certificates
        public async Task<IActionResult> Index()
        {
            var currentEmail = User.Identity?.Name;
            IQueryable<Certificate> certificatesQuery = _context.Certificates
                .Include(c => c.CertificationTrack)
                .Include(c => c.Trainee)
                .AsQueryable();

            if (User.IsInRole(RoleNames.Trainee) && !User.IsInRole(RoleNames.TrainingCoordinator))
            {
                certificatesQuery = certificatesQuery.Where(c => c.Trainee != null && c.Trainee.Email == currentEmail);
            }

            var certificates = await certificatesQuery.ToListAsync();

            var passCounts = new Dictionary<int, int>();
            var totalRequired = new Dictionary<int, int>();
            var remaining = new Dictionary<int, int>();
            var isEligible = new Dictionary<int, bool>();

            foreach (var certificate in certificates)
            {
                var evaluation = await EvaluateCertificateAsync(certificate.TraineeId, certificate.CertificationTrackId, includeExistingCertificate: false);

                passCounts[certificate.Id] = evaluation.PassedCourses;
                totalRequired[certificate.Id] = evaluation.TotalRequiredCourses;
                remaining[certificate.Id] = evaluation.RemainingCourses;
                isEligible[certificate.Id] = evaluation.IsEligible;
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

            if (User.IsInRole(RoleNames.Trainee) && !User.IsInRole(RoleNames.TrainingCoordinator))
            {
                if (!string.Equals(certificate.Trainee?.Email, User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }
            }

            var evaluation = await EvaluateCertificateAsync(certificate.TraineeId, certificate.CertificationTrackId, includeExistingCertificate: false);

            ViewBag.PassedCourses = evaluation.PassedCourses;
            ViewBag.TotalRequired = evaluation.TotalRequiredCourses;
            ViewBag.RemainingCourses = evaluation.RemainingCourses;
            ViewBag.IsEligible = evaluation.IsEligible;
            ViewBag.IsLocked = IsLockedCertificate(certificate);
            ViewBag.EvaluationMessage = evaluation.ErrorMessage;

            return View(certificate);
        }

        // GET: Certificates/Create
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        public async Task<IActionResult> Create(int? traineeId = null, int? certificationTrackId = null)
        {
            await PopulateCertificateSelectListsAsync(traineeId, certificationTrackId);

            if (traineeId.HasValue && certificationTrackId.HasValue)
            {
                var evaluation = await EvaluateCertificateAsync(traineeId.Value, certificationTrackId.Value);

                if (evaluation.ExistingCertificate != null)
                {
                    return RedirectToAction(nameof(Details), new { id = evaluation.ExistingCertificate.Id });
                }

                ViewBag.PassedCourses = evaluation.PassedCourses;
                ViewBag.TotalRequired = evaluation.TotalRequiredCourses;
                ViewBag.RemainingCourses = evaluation.RemainingCourses;
                ViewBag.IsEligible = evaluation.IsEligible;
                ViewBag.EvaluationMessage = evaluation.ErrorMessage;
            }

            return View();
        }

        // POST: Certificates/Create
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TraineeId,CertificationTrackId")] Certificate certificate)
        {
            if (certificate.TraineeId == 0 || certificate.CertificationTrackId == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select trainee and certification track.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var evaluation = await EvaluateCertificateAsync(certificate.TraineeId, certificate.CertificationTrackId);

            if (evaluation.Trainee == null || evaluation.CertificationTrack == null)
            {
                ModelState.AddModelError(string.Empty, evaluation.ErrorMessage ?? "No certification data found.");
            }
            else if (evaluation.ExistingCertificate != null)
            {
                await transaction.RollbackAsync();
                return RedirectToAction(nameof(Details), new { id = evaluation.ExistingCertificate.Id });
            }
            else if (!evaluation.IsEligible)
            {
                ModelState.AddModelError(string.Empty, evaluation.ErrorMessage ?? "Not eligible for certification yet.");
            }

            if (!ModelState.IsValid)
            {
                await transaction.RollbackAsync();
                await PopulateCertificateSelectListsAsync(certificate.TraineeId, certificate.CertificationTrackId);
                ViewBag.PassedCourses = evaluation.PassedCourses;
                ViewBag.TotalRequired = evaluation.TotalRequiredCourses;
                ViewBag.RemainingCourses = evaluation.RemainingCourses;
                ViewBag.IsEligible = evaluation.IsEligible;
                ViewBag.EvaluationMessage = evaluation.ErrorMessage;
                return View(certificate);
            }

            var duplicate = await _context.Certificates.AnyAsync(c => c.TraineeId == certificate.TraineeId && c.CertificationTrackId == certificate.CertificationTrackId);

            if (duplicate)
            {
                await transaction.RollbackAsync();
                var existingCertificate = await _context.Certificates.AsNoTracking().FirstOrDefaultAsync(c => c.TraineeId == certificate.TraineeId && c.CertificationTrackId == certificate.CertificationTrackId);

                if (existingCertificate != null)
                {
                    return RedirectToAction(nameof(Details), new { id = existingCertificate.Id });
                }

                ModelState.AddModelError(string.Empty, "This trainee already has a certificate for the selected certification track.");
                await PopulateCertificateSelectListsAsync(certificate.TraineeId, certificate.CertificationTrackId);
                ViewBag.PassedCourses = evaluation.PassedCourses;
                ViewBag.TotalRequired = evaluation.TotalRequiredCourses;
                ViewBag.RemainingCourses = evaluation.RemainingCourses;
                ViewBag.IsEligible = evaluation.IsEligible;
                ViewBag.EvaluationMessage = evaluation.ErrorMessage;
                return View(certificate);
            }

            certificate.CertificateReferenceNumber = await GenerateUniqueCertificateReferenceAsync();
            certificate.Status = "Issued";
            certificate.IssuedAt = DateTime.UtcNow;

            _context.Add(certificate);

            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Unable to issue the certificate right now. Please try again.");
                await PopulateCertificateSelectListsAsync(certificate.TraineeId, certificate.CertificationTrackId);
                ViewBag.PassedCourses = evaluation.PassedCourses;
                ViewBag.TotalRequired = evaluation.TotalRequiredCourses;
                ViewBag.RemainingCourses = evaluation.RemainingCourses;
                ViewBag.IsEligible = evaluation.IsEligible;
                ViewBag.EvaluationMessage = evaluation.ErrorMessage;
                return View(certificate);
            }

            return RedirectToAction(nameof(Details), new { id = certificate.Id });
        }

        // GET: Certificates/Edit/5
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
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

            ViewBag.IsLocked = IsLockedCertificate(certificate);
            ViewData["CertificationTrackId"] = new SelectList(_context.CertificationTracks, "Id", "Description", certificate.CertificationTrackId);
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email", certificate.TraineeId);
            return View(certificate);
        }

        // POST: Certificates/Edit/5
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TraineeId,CertificationTrackId,CertificateReferenceNumber,IssuedAt,Status")] Certificate certificate)
        {
            if (id != certificate.Id)
            {
                return NotFound();
            }

            var existingCertificate = await _context.Certificates.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

            if (existingCertificate == null)
            {
                return NotFound();
            }

            if (IsLockedCertificate(existingCertificate))
            {
                ModelState.AddModelError(string.Empty, "Issued certificates are locked and cannot be edited.");
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

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.IsLocked = IsLockedCertificate(existingCertificate);
            ViewData["CertificationTrackId"] = new SelectList(_context.CertificationTracks, "Id", "Description", certificate.CertificationTrackId);
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email", certificate.TraineeId);
            return View(certificate);
        }

        // GET: Certificates/Delete/5
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
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

            ViewBag.IsLocked = IsLockedCertificate(certificate);
            return View(certificate);
        }

        // POST: Certificates/Delete/5
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var certificate = await _context.Certificates.FindAsync(id);

            if (certificate == null)
            {
                return NotFound();
            }

            if (IsLockedCertificate(certificate))
            {
                ModelState.AddModelError(string.Empty, "Issued certificates cannot be deleted. Mark them as revoked instead.");
                ViewBag.IsLocked = true;
                return View("Delete", certificate);
            }

            try
            {
                _context.Certificates.Remove(certificate);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to delete certificate because related records exist.");
                ViewBag.IsLocked = IsLockedCertificate(certificate);
                return View("Delete", certificate);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Lookup()
        {
            return View(new CertificateLookupViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lookup(CertificateLookupViewModel model)
        {
            model.ReferenceNumber = model.ReferenceNumber?.Trim();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("TrainingHubApi");
            var requestPath = $"api/Certificates/Lookup?traineeId={model.TraineeId}&reference={Uri.EscapeDataString(model.ReferenceNumber!)}";

            try
            {
                using var response = await client.GetAsync(requestPath);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    model.ErrorMessage = "No certificate matched that trainee ID and reference number.";
                    return View(model);
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    model.ErrorMessage = "Enter a valid trainee ID and certificate reference number.";
                    return View(model);
                }

                if (!response.IsSuccessStatusCode)
                {
                    model.ErrorMessage = "Certificate verification is temporarily unavailable. Please try again.";
                    return View(model);
                }

                var certificate = await response.Content.ReadFromJsonAsync<PublicCertificateDetailsViewModel>();

                if (certificate == null)
                {
                    model.ErrorMessage = "Certificate verification returned an invalid response. Please try again.";
                    return View(model);
                }

                return View("PublicDetails", certificate);
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage = "Certificate verification service is unavailable. Please try again.";
                return View(model);
            }
        }

        private bool CertificateExists(int id)
        {
            return _context.Certificates.Any(e => e.Id == id);
        }

        private async Task PopulateCertificateSelectListsAsync(int? traineeId = null, int? certificationTrackId = null)
        {
            ViewData["CertificationTrackId"] = new SelectList(
                await _context.CertificationTracks
                    .AsNoTracking()
                    .Where(ct => ct.IsActive)
                    .OrderBy(ct => ct.Name)
                    .ToListAsync(),
                "Id",
                "Description",
                certificationTrackId);

            ViewData["TraineeId"] = new SelectList(
                await _context.Trainees
                    .AsNoTracking()
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.FullName)
                    .ToListAsync(),
                "Id",
                "Email",
                traineeId);
        }

        private async Task<CertificateEvaluationResult> EvaluateCertificateAsync(int traineeId, int certificationTrackId, bool includeExistingCertificate = true)
        {
            var trainee = await _context.Trainees
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == traineeId);

            if (trainee == null)
            {
                return new CertificateEvaluationResult(null, null, null, 0, 0, 0, false, "Trainee not found.");
            }

            if (!trainee.IsActive)
            {
                return new CertificateEvaluationResult(trainee, null, null, 0, 0, 0, false, "Trainee is inactive and cannot receive a certificate.");
            }

            var track = await _context.CertificationTracks
                .AsNoTracking()
                .Include(ct => ct.CertificationTrackCourses)
                .FirstOrDefaultAsync(ct => ct.Id == certificationTrackId);

            if (track == null)
            {
                return new CertificateEvaluationResult(trainee, null, null, 0, 0, 0, false, "Certification track not found.");
            }

            if (!track.IsActive)
            {
                return new CertificateEvaluationResult(trainee, track, null, 0, 0, 0, false, "This certification track is inactive.");
            }

            var requiredCourseIds = track.CertificationTrackCourses
                .Select(ctc => ctc.CourseId)
                .Distinct()
                .ToList();

            var totalRequired = requiredCourseIds.Count;

            var passedCourses = await _context.Enrollments
                .AsNoTracking()
                .Include(e => e.CourseSession)
                .Where(e => e.TraineeId == traineeId
                            && e.Status == "Completed"
                            && e.ResultStatus == "Pass"
                            && e.CourseSession != null
                            && requiredCourseIds.Contains(e.CourseSession.CourseId))
                .Select(e => e.CourseSession!.CourseId)
                .Distinct()
                .CountAsync();

            var remainingCourses = Math.Max(0, totalRequired - passedCourses);
            var isEligible = totalRequired > 0 && passedCourses >= totalRequired;
            var existingCertificate = includeExistingCertificate
                ? await _context.Certificates
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.TraineeId == traineeId && c.CertificationTrackId == certificationTrackId)
                : null;

            string? errorMessage = null;

            if (totalRequired == 0)
            {
                errorMessage = "No certification data found for this track.";
            }
            else if (!isEligible)
            {
                errorMessage = "Not eligible for certification yet.";
            }

            return new CertificateEvaluationResult(trainee, track, existingCertificate, passedCourses, totalRequired, remainingCourses, isEligible, errorMessage);
        }

        private static bool IsLockedCertificate(Certificate certificate)
        {
            return string.Equals(certificate.Status, "Issued", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<string> GenerateUniqueCertificateReferenceAsync()
        {
            for (var attempt = 0; attempt < 5; attempt++)
            {
                var reference = GenerateCertificateReference();
                var exists = await _context.Certificates.AnyAsync(c => c.CertificateReferenceNumber == reference);

                if (!exists)
                {
                    return reference;
                }
            }

            throw new InvalidOperationException("Unable to generate a unique certificate reference number.");
        }

        private string GenerateCertificateReference()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = Random.Shared.Next(1000, 9999);
            return $"CERT-{timestamp}-{random}";
        }

        private sealed record CertificateEvaluationResult(
            Trainee? Trainee,
            CertificationTrack? CertificationTrack,
            Certificate? ExistingCertificate,
            int PassedCourses,
            int TotalRequiredCourses,
            int RemainingCourses,
            bool IsEligible,
            string? ErrorMessage);
    }
}

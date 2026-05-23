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
    public class EnrollmentsController : Controller
    {
        private static readonly HashSet<string> AllowedEnrollmentStatuses = new(StringComparer.Ordinal)
        {
            "Enrolled",
            "Confirmed",
            "Attending",
            "Completed",
            "Dropped"
        };

        private static readonly HashSet<string> AllowedAttendanceStatuses = new(StringComparer.Ordinal)
        {
            "Pending",
            "Present",
            "Absent"
        };

        private static readonly HashSet<string> AllowedResultStatuses = new(StringComparer.Ordinal)
        {
            "Pending",
            "Pass",
            "Fail"
        };

        private readonly TrainingHubDbContext _context;

        public EnrollmentsController(TrainingHubDbContext context)
        {
            _context = context;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index()
        {
            var trainingHubDbContext = _context.Enrollments
                .Include(e => e.CourseSession)
                .Include(e => e.Trainee);

            return View(await trainingHubDbContext.ToListAsync());
        }

        // GET: Enrollments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var enrollment = await _context.Enrollments
                .Include(e => e.CourseSession)
                .Include(e => e.Trainee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (enrollment == null)
                return NotFound();

            return View(enrollment);
        }

        // GET: Enrollments/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: Enrollments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TraineeId,CourseSessionId,Status,EnrolledAt,AttendanceStatus,ResultStatus,ResultRecordedAt")] Enrollment enrollment)
        {
            ValidateEnrollmentLifecycleValues(enrollment);
            await ValidateEnrollmentRules(enrollment);

            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(enrollment);
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var enrollment = await _context.Enrollments.FindAsync(id);

            if (enrollment == null)
                return NotFound();

            PopulateDropdowns(enrollment);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TraineeId,CourseSessionId,Status,EnrolledAt,AttendanceStatus,ResultStatus,ResultRecordedAt")] Enrollment enrollment)
        {
            if (id != enrollment.Id)
                return NotFound();

            ValidateEnrollmentLifecycleValues(enrollment);
            await ValidateEnrollmentRules(enrollment);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(enrollment);
            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var enrollment = await _context.Enrollments
                .Include(e => e.CourseSession)
                .Include(e => e.Trainee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (enrollment == null)
                return NotFound();

            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);

            if (enrollment == null)
                return NotFound();

            try
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete enrollment because related records exist.");
                return View("Delete", enrollment);
            }
        }

        private async Task ValidateEnrollmentRules(Enrollment enrollment)
        {
            var duplicateEnrollment = await _context.Enrollments.AnyAsync(e =>
                e.TraineeId == enrollment.TraineeId &&
                e.CourseSessionId == enrollment.CourseSessionId &&
                e.Id != enrollment.Id);

            if (duplicateEnrollment)
            {
                ModelState.AddModelError("", "This trainee is already enrolled in this session.");
            }

            var session = await _context.CourseSessions
                .Include(cs => cs.Enrollments)
                .FirstOrDefaultAsync(cs => cs.Id == enrollment.CourseSessionId);

            if (session == null)
            {
                ModelState.AddModelError("", "Selected course session does not exist.");
                return;
            }

            if (session.Enrollments.Count(e => e.Id != enrollment.Id) >= session.Capacity)
            {
                ModelState.AddModelError("", "This session is already full.");
            }

            var selectedSession = await _context.CourseSessions
                .Include(cs => cs.Course)
                .FirstOrDefaultAsync(cs => cs.Id == enrollment.CourseSessionId);

            if (selectedSession?.Course?.PrerequisiteCourseId != null)
            {
                var prerequisiteCompleted = await _context.Enrollments
                    .Include(e => e.CourseSession)
                    .AnyAsync(e =>
                        e.TraineeId == enrollment.TraineeId &&
                        e.CourseSession!.CourseId == selectedSession.Course.PrerequisiteCourseId &&
                        e.Status == "Completed" &&
                        e.ResultStatus == "Pass");

                if (!prerequisiteCompleted)
                {
                    ModelState.AddModelError(
                        "",
                        "Trainee has not completed the prerequisite course.");
                }
            }
        }

        private void ValidateEnrollmentLifecycleValues(Enrollment enrollment)
        {
            if (!AllowedEnrollmentStatuses.Contains(enrollment.Status))
            {
                ModelState.AddModelError(nameof(Enrollment.Status), "Select a valid enrollment status.");
            }

            if (!AllowedAttendanceStatuses.Contains(enrollment.AttendanceStatus))
            {
                ModelState.AddModelError(nameof(Enrollment.AttendanceStatus), "Select a valid attendance status.");
            }

            if (!AllowedResultStatuses.Contains(enrollment.ResultStatus))
            {
                ModelState.AddModelError(nameof(Enrollment.ResultStatus), "Select a valid result status.");
            }
        }

        private void PopulateDropdowns(Enrollment? enrollment = null)
        {
            var sessions = _context.CourseSessions
                .Include(cs => cs.Course)
                .ToList();

            var sessionList = sessions.Select(cs => new
            {
                Id = cs.Id,
                Name = $"Session {cs.Id} - {cs.Course?.Title}"
            });

            ViewData["CourseSessionId"] = new SelectList(
                sessionList,
                "Id",
                "Name",
                enrollment?.CourseSessionId);

            ViewData["TraineeId"] = new SelectList(
                _context.Trainees,
                "Id",
                "Email",
                enrollment?.TraineeId);
        }

        private bool EnrollmentExists(int id)
        {
            return _context.Enrollments.Any(e => e.Id == id);
        }
    }
}